using Backend_Boarding_house_management_system.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend_Boarding_house_management_system.Extensions
{
    public static class AspectAnalysisExtensions
    {
        /// <summary>
        /// Non-fatal startup check for the external Python ABSA microservice.
        /// Mirrors the pattern used for Cloudinary (warning on failure, never crashes the app).
        /// The AspectAnalysisService already has keyword fallback, so a down model service is acceptable.
        /// </summary>
        public static async Task<WebApplication> CheckAspectAnalysisConnectionAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("AspectAnalysisConnectionCheck");

                var options = scope.ServiceProvider.GetService<IOptions<AspectAnalysisOptions>>()?.Value
                              ?? new AspectAnalysisOptions();

                if (!options.Enabled || string.IsNullOrWhiteSpace(options.PythonServiceUrl))
                {
                    logger.LogInformation("I [INFO] AspectAnalysis disabled or no PythonServiceUrl configured. Using keyword fallback only.");
                    return app;
                }

                try
                {
                    // We send a minimal valid payload to the /analyze endpoint (or /health if you prefer a dedicated ping).
                    // Using a tiny review + stars.
                    var analyzeUrl = options.PythonServiceUrl;
                    // If user pointed at the analyze route or the root service, we try analyze first then fall back to health.
                    var healthUrl = analyzeUrl.Replace("/analyze", "/health", StringComparison.OrdinalIgnoreCase);

                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(Math.Max(3, Math.Min(options.RequestTimeoutSeconds, 15)))
                    };

                    // Prefer /health (lightweight)
                    var response = await httpClient.GetAsync(healthUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation("O [SUCCESS] AspectAnalysis Python service reachable at {Url}", healthUrl);
                        return app;
                    }

                    // Fallback: try a real analyze call (some deployments may not expose /health publicly)
                    var payload = new
                    {
                        content = "Phòng sạch, wifi ổn.",
                        stars = 4
                    };
                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var analyzeResp = await httpClient.PostAsync(analyzeUrl, content);
                    if (analyzeResp.IsSuccessStatusCode)
                    {
                        logger.LogInformation("O [SUCCESS] AspectAnalysis Python service reachable via analyze at {Url}", analyzeUrl);
                    }
                    else
                    {
                        var err = await analyzeResp.Content.ReadAsStringAsync();
                        logger.LogWarning("I [WARNING] AspectAnalysis service responded with {Status}. Details: {Err}. Keyword fallback will be used.", analyzeResp.StatusCode, err);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "X [WARNING] Could not reach AspectAnalysis Python service at configured URL. Keyword-based fallback will be active for all ratings. This is expected during development if the Python service is not started.");
                }
            }

            return app;
        }
    }
}
