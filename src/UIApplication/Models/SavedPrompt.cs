namespace UIApplication.Models
{
    public class SavedPrompt
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Exchange { get; set;} = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class SavedPromptsData
    {
        public List<SavedPrompt> SavedItems { get; set; } = new();
    }
}
