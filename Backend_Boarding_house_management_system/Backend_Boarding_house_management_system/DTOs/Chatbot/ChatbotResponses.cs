using System.Text.Json.Serialization;

namespace Backend_Boarding_house_management_system.DTOs.Chatbot.Responses
{
    public class ChatResponse
    {
        [JsonPropertyName("reply")]
        public string Reply { get; set; } = string.Empty;
        
        [JsonPropertyName("emotion")]
        public EmotionResult Emotion { get; set; } = new();
        
        [JsonPropertyName("suggestions")]
        public List<string> Suggestions { get; set; } = new();

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("context")]
        public string Context { get; set; } = string.Empty;
    }

    public class EmotionResult
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = "neutral";
        
        [JsonPropertyName("label_vi")]
        public string LabelVi { get; set; } = "bình thường";
        
        [JsonPropertyName("score")]
        public double Score { get; set; } = 0.0;
        
        [JsonPropertyName("urgency")]
        public string Urgency { get; set; } = "low";
        
        [JsonPropertyName("source")]
        public string Source { get; set; } = "rule-based";
        
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        
        [JsonPropertyName("color")]
        public string Color { get; set; } = "#94a3b8";

        [JsonPropertyName("all_scores")]
        public Dictionary<string, double> AllScores { get; set; } = new();
    }

    public class ComplaintAnalysisResponse
    {
        public EmotionResult Emotion { get; set; } = new();
        public string Urgency { get; set; } = "low";
        public string SuggestedCategory { get; set; } = string.Empty;
        public string SuggestedResponse { get; set; } = string.Empty;
    }
}
