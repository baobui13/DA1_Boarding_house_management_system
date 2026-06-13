using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Options;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend_Boarding_house_management_system.Services.Implements.Recommendation
{
    /// <summary>
    /// Scorer chính dựa trên Profile/Mode.
    /// Hỗ trợ nhiều RecommendationMode với trọng số và logic khác nhau.
    /// Dễ dàng mở rộng thêm mode mới hoặc override weights từ config.
    /// </summary>
    public class ProfileBasedPropertyScorer : IPropertyScorer
    {
        private readonly RecommendationOptions _options;

        public ProfileBasedPropertyScorer(IOptions<RecommendationOptions> options)
        {
            _options = options?.Value ?? new RecommendationOptions();
        }

        public double CalculateScore(
            Property property,
            UserPreference? preference,
            HashSet<ReviewAspect>? userPositiveAspects,
            HashSet<ReviewAspect>? userNegativeAspects,
            RecommendationMode mode,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null,
            string? userId = null)
        {
            bool isLoggedIn = !string.IsNullOrEmpty(userId);
            var weights = ResolveWeights(mode);

            double score = weights.BaseScore;

            // === 1. History signals (area, price, amenity) ===
            double historyContribution = CalculateHistoryContribution(property, preference, weights);

            // Áp dụng trọng số history khác nhau tùy mode
            double historyMultiplier = GetHistoryMultiplier(mode, isLoggedIn, weights);
            score += historyContribution * historyMultiplier;

            // === 2. Aspect signals (tận dụng PropertyAspectScore từ ABSA) ===
            double aspectContribution = CalculateAspectContribution(
                property,
                userPositiveAspects ?? new HashSet<ReviewAspect>(),
                userNegativeAspects ?? new HashSet<ReviewAspect>(),
                mode,
                weights,
                searchAspectBoosts);

            score += aspectContribution * weights.AspectMatch;

            // === 3. Mode-specific adjustments ===
            score = ApplyModeSpecificAdjustments(score, property, mode, weights, userPositiveAspects, userNegativeAspects);

            return Math.Max(0, score);
        }

        private ScoringWeights ResolveWeights(RecommendationMode mode)
        {
            // Ưu tiên profile cụ thể trong config
            string modeKey = mode.ToString();
            if (_options.Profiles.TryGetValue(modeKey, out var profileWeights))
            {
                // Merge với base (profile override base)
                return MergeWeights(_options.BaseWeights, profileWeights);
            }

            // Nếu không có profile config thì tạo weights mặc định cho mode
            return GetDefaultWeightsForMode(mode, _options.BaseWeights);
        }

        private ScoringWeights MergeWeights(ScoringWeights baseW, ScoringWeights overrideW)
        {
            return new ScoringWeights
            {
                PersonalHistory = overrideW.PersonalHistory != 0 ? overrideW.PersonalHistory : baseW.PersonalHistory,
                GlobalHistory = overrideW.GlobalHistory != 0 ? overrideW.GlobalHistory : baseW.GlobalHistory,
                AspectMatch = overrideW.AspectMatch != 0 ? overrideW.AspectMatch : baseW.AspectMatch,
                AspectGlobalQuality = overrideW.AspectGlobalQuality != 0 ? overrideW.AspectGlobalQuality : baseW.AspectGlobalQuality,
                PriceFit = overrideW.PriceFit != 0 ? overrideW.PriceFit : baseW.PriceFit,
                AreaMatch = overrideW.AreaMatch != 0 ? overrideW.AreaMatch : baseW.AreaMatch,
                AmenityMatchPerItem = overrideW.AmenityMatchPerItem != 0 ? overrideW.AmenityMatchPerItem : baseW.AmenityMatchPerItem,
                MaxAmenityBonus = overrideW.MaxAmenityBonus != 0 ? overrideW.MaxAmenityBonus : baseW.MaxAmenityBonus,
                NegativeAspectPenaltyMultiplier = overrideW.NegativeAspectPenaltyMultiplier != 0 ? overrideW.NegativeAspectPenaltyMultiplier : baseW.NegativeAspectPenaltyMultiplier,
                LowEvidencePenalty = overrideW.LowEvidencePenalty != 0 ? overrideW.LowEvidencePenalty : baseW.LowEvidencePenalty,
                BaseScore = overrideW.BaseScore != 0 ? overrideW.BaseScore : baseW.BaseScore
            };
        }

        private ScoringWeights GetDefaultWeightsForMode(RecommendationMode mode, ScoringWeights baseWeights)
        {
            var w = new ScoringWeights
            {
                BaseScore = baseWeights.BaseScore,
                PersonalHistory = baseWeights.PersonalHistory,
                GlobalHistory = baseWeights.GlobalHistory,
                AspectMatch = baseWeights.AspectMatch,
                AspectGlobalQuality = baseWeights.AspectGlobalQuality,
                PriceFit = baseWeights.PriceFit,
                AreaMatch = baseWeights.AreaMatch,
                AmenityMatchPerItem = baseWeights.AmenityMatchPerItem,
                MaxAmenityBonus = baseWeights.MaxAmenityBonus,
                NegativeAspectPenaltyMultiplier = baseWeights.NegativeAspectPenaltyMultiplier,
                LowEvidencePenalty = baseWeights.LowEvidencePenalty
            };

            switch (mode)
            {
                case RecommendationMode.HighAspectQuality:
                    w.AspectMatch = Math.Max(w.AspectMatch, 3.5);
                    w.AspectGlobalQuality = 1.0;
                    w.PersonalHistory = 0.3;
                    w.GlobalHistory = 0.2;
                    w.LowEvidencePenalty = 8; // phạt nhẹ nếu ít review
                    break;

                case RecommendationMode.Balanced:
                    w.AspectMatch = 2.0;
                    w.PersonalHistory = 1.0;
                    w.GlobalHistory = 0.7;
                    break;

                case RecommendationMode.PriceSensitive:
                    w.PriceFit = Math.Max(w.PriceFit, 55);
                    w.AspectMatch = 1.2;
                    w.PersonalHistory = 0.8;
                    break;

                case RecommendationMode.Explore:
                    w.PersonalHistory = 0.2;
                    w.GlobalHistory = 0.8;
                    w.AspectMatch = 1.8;
                    w.AspectGlobalQuality = 0.9;
                    // Giảm ảnh hưởng personal để khuyến khích đa dạng
                    break;

                case RecommendationMode.AvoidNegatives:
                    w.NegativeAspectPenaltyMultiplier = Math.Max(w.NegativeAspectPenaltyMultiplier, 2.5);
                    w.AspectMatch = 2.2;
                    break;

                case RecommendationMode.PersonalMatch:
                default:
                    // Giữ nguyên hành vi cũ (gần với logic cũ trong PropertyService)
                    // isLoggedIn sẽ được xử lý bên ngoài qua multiplier
                    break;
            }

            return w;
        }

        private double GetHistoryMultiplier(RecommendationMode mode, bool isLoggedIn, ScoringWeights weights)
        {
            double wPersonal = isLoggedIn ? weights.PersonalHistory : 0.0;
            double wGlobal = isLoggedIn ? weights.GlobalHistory : 1.1; // anonymous thì đẩy global mạnh hơn

            // Một số mode override mạnh
            if (mode == RecommendationMode.Explore)
            {
                wPersonal *= 0.3;
                wGlobal *= 1.3;
            }
            else if (mode == RecommendationMode.HighAspectQuality)
            {
                wPersonal *= 0.4;
            }

            return (wPersonal * 0.85 + wGlobal * 0.15);
        }

        private double CalculateHistoryContribution(Property property, UserPreference? pref, ScoringWeights weights)
        {
            double contribution = 0;

            if (pref == null)
                return contribution;

            // Area
            if (!string.IsNullOrEmpty(property.AreaId) && pref.AreaIds.Contains(property.AreaId))
                contribution += weights.AreaMatch;

            // Price fit
            if ((pref.PriceMean.HasValue || (pref.PriceMin.HasValue && pref.PriceMax.HasValue)))
            {
                decimal target = pref.PriceMean ?? ((pref.PriceMin!.Value + pref.PriceMax!.Value) / 2);
                decimal distance = Math.Abs(property.Price - target);
                decimal tolerance = Math.Max(300_000, target * 0.25m);
                double priceFit = Math.Max(0, 1 - (double)(distance / tolerance));
                contribution += priceFit * weights.PriceFit;
            }

            // Amenity
            int matchCount = 0;
            foreach (var ra in property.RoomAmenities)
            {
                if (string.Equals(ra.Status, "Working", StringComparison.OrdinalIgnoreCase) &&
                    pref.AmenityIds.Contains(ra.AmenityId))
                    matchCount++;
            }
            if (matchCount > 0)
                contribution += Math.Min(matchCount * weights.AmenityMatchPerItem, weights.MaxAmenityBonus);

            return contribution;
        }

        private double CalculateAspectContribution(
            Property property,
            HashSet<ReviewAspect> positiveAspects,
            HashSet<ReviewAspect> negativeAspects,
            RecommendationMode mode,
            ScoringWeights weights,
            IDictionary<ReviewAspect, double>? searchAspectBoosts = null)
        {
            if (property.PropertyAspectScores == null || !property.PropertyAspectScores.Any())
                return 0;

            var scores = property.PropertyAspectScores.ToList();
            double avgWeighted = scores.Average(s => (double)s.WeightedScore);

            double contribution = avgWeighted * weights.AspectGlobalQuality;

            // Personal positive match (từ lịch sử rating/view)
            if (positiveAspects.Count > 0)
            {
                double personalBonus = 0;
                foreach (var pas in scores)
                {
                    if (positiveAspects.Contains(pas.Aspect) && pas.WeightedScore > 50)
                    {
                        personalBonus += (double)pas.WeightedScore / 6.5;
                    }
                }
                contribution += personalBonus * 0.9;
            }

            // === MỚI: Aspect boost từ search hiện tại của user ===
            // User điền thêm aspect khi search → property có WeightedScore cao trên những aspect này sẽ được cộng thêm điểm mạnh.
            if (searchAspectBoosts != null && searchAspectBoosts.Count > 0)
            {
                foreach (var kvp in searchAspectBoosts)
                {
                    var asp = kvp.Key;
                    var boost = kvp.Value > 0 ? kvp.Value : 1.0;

                    var pas = scores.FirstOrDefault(s => s.Aspect == asp);
                    if (pas != null)
                    {
                        // Càng cao WeightedScore → càng được thưởng nhiều theo boost factor
                        // Hệ số 0.4 để không lấn át toàn bộ điểm, có thể tinh chỉnh
                        double searchBonus = (double)pas.WeightedScore * boost * 0.4;
                        contribution += searchBonus;
                    }
                }
            }

            // === AvoidNegatives mode: penalty mạnh ===
            if (mode == RecommendationMode.AvoidNegatives && negativeAspects.Count > 0)
            {
                double penalty = 0;
                foreach (var pas in scores)
                {
                    if (negativeAspects.Contains(pas.Aspect))
                    {
                        // Property càng yếu (WeightedScore thấp) trên aspect user ghét → phạt càng nặng
                        double weakness = Math.Max(0, 60 - (double)pas.WeightedScore);
                        penalty += weakness * weights.NegativeAspectPenaltyMultiplier;
                    }
                }
                contribution -= penalty;
            }

            // Low evidence penalty (chủ yếu cho HighAspectQuality)
            if (weights.LowEvidencePenalty > 0)
            {
                int totalEvidence = scores.Sum(s => s.TotalCount);
                if (totalEvidence < 3)
                    contribution -= weights.LowEvidencePenalty;
            }

            return contribution;
        }

        private double ApplyModeSpecificAdjustments(
            double currentScore,
            Property property,
            RecommendationMode mode,
            ScoringWeights weights,
            HashSet<ReviewAspect>? positive,
            HashSet<ReviewAspect>? negative)
        {
            switch (mode)
            {
                case RecommendationMode.HighAspectQuality:
                    // Thưởng thêm nếu có nhiều aspect có WeightedScore cao
                    if (property.PropertyAspectScores != null)
                    {
                        int strongAspects = property.PropertyAspectScores.Count(s => s.WeightedScore >= 75);
                        currentScore += strongAspects * 4;
                    }
                    break;

                case RecommendationMode.Explore:
                    // Thưởng nhẹ cho property mới (để đa dạng)
                    var daysOld = (DateTime.UtcNow - property.CreatedAt).TotalDays;
                    if (daysOld < 14)
                        currentScore += 12;
                    break;

                case RecommendationMode.AvoidNegatives:
                    // Đã xử lý penalty trong CalculateAspectContribution
                    break;

                // Các mode khác giữ nguyên hoặc có thể mở rộng sau
            }

            return currentScore;
        }
    }
}
