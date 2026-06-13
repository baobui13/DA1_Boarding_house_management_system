namespace Backend_Boarding_house_management_system.Options
{
    /// <summary>
    /// Configuration for the real ABSA (two-head PhoBERT) analysis service.
    /// When Enabled + PythonServiceUrl is set, the AspectAnalysisService will call the Python microservice.
    /// Otherwise (or on failure) it falls back to the built-in keyword-based logic.
    /// </summary>
    public class AspectAnalysisOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Full URL to the Python /analyze endpoint, e.g. http://localhost:8001/analyze
        /// </summary>
        public string? PythonServiceUrl { get; set; }

        public int RequestTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Only forwarded to the Python service (it decides "mentioned"). Not used in keyword fallback.
        /// </summary>
        public double AspectPresenceThreshold { get; set; } = 0.5;

        public bool FallbackToKeywordOnError { get; set; } = true;
    }
}
