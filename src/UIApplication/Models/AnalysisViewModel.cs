namespace UIApplication.Models
{
    public class AnalysisViewModel
    {
        public string Prompt { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string? Analysis { get; set; }
        public bool IsLoading { get; set; }
        public string? ErrorMessage { get; set; }
        public List<SavedPrompt> SavedPrompts { get; set; } = new();
    }
}
