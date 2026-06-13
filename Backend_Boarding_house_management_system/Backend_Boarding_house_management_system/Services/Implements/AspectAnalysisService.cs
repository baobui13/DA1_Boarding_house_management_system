using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Options;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    /// <summary>
    /// ABSA (Aspect-Based Sentiment Analysis) implementation.
    /// 
    /// Preferred path: calls the external Python microservice that hosts the trained
    /// TwoHead_Shared_GatedFusion PhoBERT model (aspect presence head + sentiment head).
    /// 
    /// Automatic fallback (when disabled, url missing, or any error/timeout):
    /// the original keyword/rule-based logic (optimized for Vietnamese boarding-house reviews).
    /// 
    /// Results are used to create RatingAspect rows and to maintain PropertyAspectScore
    /// aggregates (enables aspect-aware property recommendation).
    /// </summary>
    public class AspectAnalysisService : IAspectAnalysisService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AspectAnalysisOptions _options;
        private readonly ILogger<AspectAnalysisService> _logger;

        // JSON options for the wire contract with Python service (small, stable DTOs)
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private record AbsaRequest(string Content, int? Stars);
        private record AbsaAspect(string Aspect, string Sentiment, decimal? Confidence);
        private record AbsaResponse(List<AbsaAspect> Aspects);

        // Từ khóa positive / negative cho từng aspect (tiếng Việt + một số tiếng Anh phổ biến)
        // Có thể mở rộng sau. (Preserved exactly for fallback path.)
        private static readonly Dictionary<ReviewAspect, (string[] Positive, string[] Negative)> _keywords = new()
        {
            [ReviewAspect.RoomQuality] = (
                new[] { "sạch", "rộng", "sáng", "thoáng", "đẹp", "mới", "tiện nghi", "gác lửng", "ban công", "rộng rãi", "khá sạch", "sạch sẽ", "không ẩm" },
                new[] { "bẩn", "chật", "tối", "ẩm", "cũ", "dơ", "hẹp", "mốc", "hôi", "xấu" }
            ),
            [ReviewAspect.Noise] = (
                new[] { "yên tĩnh", "im lặng", "yên bình", "không ồn", "yên", "im", "tĩnh" },
                new[] { "ồn", "ồn ào", "xe", "cộ", "hàng xóm", "sang", "nói", "chửi", "ồn ào" }
            ),
            [ReviewAspect.Wifi] = (
                new[] { "wifi", "mạng", "internet", "nhanh", "mạnh", "ổn định", "tốt", "wireless" },
                new[] { "wifi yếu", "chập chờn", "mất mạng", "chậm", "không vào", "lag", "kết nối kém" }
            ),
            [ReviewAspect.Utilities] = (
                new[] { "điện", "nước", "nóng", "lạnh", "đủ", "mạnh", "tiện", "cấp nước", "điện ổn" },
                new[] { "cắt điện", "mất nước", "nước yếu", "điện yếu", "hỏng", "chậm", "tiền điện cao" }
            ),
            [ReviewAspect.Parking] = (
                new[] { "để xe", "xe máy", "ô tô", "rộng", "an toàn", "có chỗ", "bãi xe", "gửi xe" },
                new[] { "không chỗ", "chật", "đắt", "khó gửi", "không an toàn", "phải để ngoài" }
            ),
            [ReviewAspect.Security] = (
                new[] { "an ninh", "khóa", "cổng", "camera", "an toàn", "gác", "bảo vệ", "khóa cửa", "tuyệt vời" },
                new[] { "không an toàn", "trộm", "mất cắp", "cửa hỏng", "không có gác", "dễ vào" }
            ),
            [ReviewAspect.Environment] = (
                new[] { "môi trường", "sạch sẽ", "xanh", "đẹp", "khu vực", "vệ sinh", "thoáng mát", "tốt" },
                new[] { "bẩn", "rác", "hôi", "ngập", "dơ", "môi trường kém" }
            ),
            [ReviewAspect.Landlord] = (
                new[] { "chủ trọ", "chủ nhà", "hỗ trợ", "nhanh", "thân thiện", "tốt", "tử tế", "giúp", "chăm sóc", "linh hoạt", "thỏa đáng" },
                new[] { "khó tính", "keo kiệt", "chậm", "không hỗ trợ", "cáu", "bảo thủ", "bắt bẻ", "lừa" }
            ),
            [ReviewAspect.Location] = (
                new[] { "vị trí", "gần", "tiện", "trường", "hutech", "đinh tiên hoàng", "đi học", "đi làm", "xe buýt", "chợ", "siêu thị", "trung tâm" },
                new[] { "xa", "bất tiện", "khó đi", "xa trường", "kẹt xe", "không tiện" }
            ),
            [ReviewAspect.Price] = (
                new[] { "rẻ", "hợp lý", "giá tốt", "đáng giá", "tiết kiệm", "phải chăng", "ok với giá" },
                new[] { "đắt", "mắc", "cao", "không đáng", "lừa giá", "đắt đỏ" }
            ),
            [ReviewAspect.Overall] = (
                new[] { "tốt", "hài lòng", "thích", "ổn", "tuyệt", "khuyên", "nên thuê", "sẽ ở lại", "đáng tiền" },
                new[] { "tệ", "không thích", "thất vọng", "không nên", "hối hận", "kém", "dở" }
            )
        };

        public AspectAnalysisService(
            IHttpClientFactory httpClientFactory,
            IOptions<AspectAnalysisOptions> options,
            ILogger<AspectAnalysisService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options?.Value ?? new AspectAnalysisOptions();
            _logger = logger;
        }

        public async Task<List<AnalyzedAspect>> AnalyzeReviewAspectsAsync(string content, int stars)
        {
            // If explicitly disabled or no URL configured → immediate keyword fallback (no network attempt)
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.PythonServiceUrl))
            {
                return await Task.FromResult(RunKeywordAnalysis(content, stars));
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("AbsaClient");

                // Respect configured timeout (with a small hard floor/ceiling for safety)
                var timeout = TimeSpan.FromSeconds(Math.Clamp(_options.RequestTimeoutSeconds, 3, 30));
                using var cts = new System.Threading.CancellationTokenSource(timeout);

                var req = new AbsaRequest(content ?? string.Empty, stars);
                var json = JsonSerializer.Serialize(req);
                using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(_options.PythonServiceUrl, httpContent, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(cts.Token);
                    _logger.LogWarning("ABSA Python service returned {Status}. Falling back to keyword analysis. Details: {Err}", response.StatusCode, err);
                    return RunKeywordAnalysis(content ?? string.Empty, stars);
                }

                var body = await response.Content.ReadAsStringAsync(cts.Token);
                var parsed = JsonSerializer.Deserialize<AbsaResponse>(body, _jsonOpts);

                if (parsed?.Aspects == null || parsed.Aspects.Count == 0)
                {
                    _logger.LogDebug("ABSA service returned no aspects. Using keyword fallback for content.");
                    return RunKeywordAnalysis(content ?? string.Empty, stars);
                }

                var results = new List<AnalyzedAspect>();
                foreach (var a in parsed.Aspects)
                {
                    if (!Enum.TryParse<ReviewAspect>(a.Aspect, ignoreCase: true, out var aspect))
                    {
                        _logger.LogDebug("Unknown aspect name from model: {Aspect}", a.Aspect);
                        continue;
                    }
                    if (!Enum.TryParse<RatingAttitude>(a.Sentiment, ignoreCase: true, out var sentiment))
                    {
                        sentiment = RatingAttitude.Neutral;
                    }
                    results.Add(new AnalyzedAspect(aspect, sentiment, a.Confidence.HasValue ? Math.Round(a.Confidence.Value, 2) : null));
                }

                // Guarantee Overall (model may or may not return it; our Python wrapper always tries)
                if (!results.Any(r => r.Aspect == ReviewAspect.Overall))
                {
                    var overallSent = stars >= 4 ? RatingAttitude.Positive : (stars <= 2 ? RatingAttitude.Negative : RatingAttitude.Neutral);
                    results.Add(new AnalyzedAspect(ReviewAspect.Overall, overallSent, 0.65m));
                }

                _logger.LogDebug("Real ABSA model produced {Count} aspects for review (len={Len})", results.Count, (content ?? "").Length);
                return results.DistinctBy(r => r.Aspect).ToList();
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("ABSA Python service call timed out after {Timeout}s. Using keyword fallback.", _options.RequestTimeoutSeconds);
                if (_options.FallbackToKeywordOnError)
                    return RunKeywordAnalysis(content, stars);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ABSA Python service call failed. Using keyword fallback (FallbackToKeywordOnError={Fallback}).", _options.FallbackToKeywordOnError);
                if (_options.FallbackToKeywordOnError)
                    return RunKeywordAnalysis(content, stars);
                throw;
            }
        }

        /// <summary>
        /// Runs the original keyword-based analysis (exact behavior preserved).
        /// Extracted so both the old direct path and the http-fallback path can use it.
        /// </summary>
        private List<AnalyzedAspect> RunKeywordAnalysis(string content, int stars)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return FallbackFromStarsOnly(stars);
            }

            var lowered = content.ToLowerInvariant();
            lowered = Regex.Replace(lowered, @"[^\p{L}\p{N}\s]", " ");

            var results = new List<AnalyzedAspect>();
            var seen = new HashSet<ReviewAspect>();

            foreach (var kvp in _keywords)
            {
                var aspect = kvp.Key;
                var (posWords, negWords) = kvp.Value;

                int posHits = CountHits(lowered, posWords);
                int negHits = CountHits(lowered, negWords);

                if (posHits == 0 && negHits == 0)
                    continue;

                RatingAttitude sentiment;
                decimal confidence;

                if (posHits > negHits)
                {
                    sentiment = RatingAttitude.Positive;
                    confidence = Math.Min(0.95m, 0.65m + (posHits - negHits) * 0.08m);
                }
                else if (negHits > posHits)
                {
                    sentiment = RatingAttitude.Negative;
                    confidence = Math.Min(0.95m, 0.65m + (negHits - posHits) * 0.08m);
                }
                else
                {
                    sentiment = stars >= 4 ? RatingAttitude.Positive :
                                stars <= 2 ? RatingAttitude.Negative : RatingAttitude.Neutral;
                    confidence = 0.55m;
                }

                if (stars >= 5 && sentiment == RatingAttitude.Neutral) sentiment = RatingAttitude.Positive;
                if (stars <= 2 && sentiment == RatingAttitude.Positive) sentiment = RatingAttitude.Negative;

                results.Add(new AnalyzedAspect(aspect, sentiment, Math.Round(confidence, 2)));
                seen.Add(aspect);
            }

            if (!seen.Contains(ReviewAspect.Overall))
            {
                var overallSentiment = stars >= 4 ? RatingAttitude.Positive :
                                       stars <= 2 ? RatingAttitude.Negative : RatingAttitude.Neutral;
                results.Add(new AnalyzedAspect(ReviewAspect.Overall, overallSentiment, 0.70m));
            }

            if (results.Count <= 2)
            {
                results.AddRange(FallbackFromStarsOnly(stars).Where(a => !seen.Contains(a.Aspect)));
            }

            return results.DistinctBy(r => r.Aspect).ToList();
        }

        private static int CountHits(string text, string[] keywords)
        {
            int count = 0;
            foreach (var kw in keywords)
            {
                if (text.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }

        private static List<AnalyzedAspect> FallbackFromStarsOnly(int stars)
        {
            var sentiment = stars >= 4 ? RatingAttitude.Positive :
                            stars <= 2 ? RatingAttitude.Negative : RatingAttitude.Neutral;

            // Trả về Overall + 1-2 aspect phổ biến khác (bias tích cực nếu sao cao)
            var list = new List<AnalyzedAspect>
            {
                new(ReviewAspect.Overall, sentiment, 0.65m)
            };

            if (stars >= 4)
            {
                list.Add(new(ReviewAspect.RoomQuality, sentiment, 0.60m));
                list.Add(new(ReviewAspect.Location, sentiment, 0.55m));
            }
            else if (stars <= 2)
            {
                list.Add(new(ReviewAspect.RoomQuality, sentiment, 0.60m));
                list.Add(new(ReviewAspect.Landlord, sentiment, 0.55m));
            }
            else
            {
                list.Add(new(ReviewAspect.Utilities, RatingAttitude.Neutral, 0.50m));
            }

            return list;
        }
    }
}
