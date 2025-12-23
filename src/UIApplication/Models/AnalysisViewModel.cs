using UIApplication.Data;

namespace UIApplication.Models
{
    public class AnalysisViewModel
    {
        public string Prompt { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
        public string? Analysis { get; set; }
        public bool IsLoading { get; set; }
        public string? ErrorMessage { get; set; }
        public List<UserPrompt> SavedPrompts { get; set; } = new();
        public List<SavedPrompt> LocalSavedPrompts { get; set; } = new();
        public int? UsedPromptId { get; set; }
    }
}
