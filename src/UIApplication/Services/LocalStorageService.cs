using System.Text.Json;
using UIApplication.Models;

namespace UIApplication.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly string _filePath;
        private readonly ILogger<LocalStorageService> _logger;

        public LocalStorageService(ILogger<LocalStorageService> logger)
        {
            _logger = logger;
            
            // Get user's AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "StockAnalysisApp");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _filePath = Path.Combine(appFolder, "saved_prompts.json");
            _logger.LogInformation("Local storage file path: {FilePath}", _filePath);
        }

        public async Task<List<SavedPrompt>> GetSavedPromptsAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogInformation("No saved prompts file found, returning empty list");
                    return new List<SavedPrompt>();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                var data = JsonSerializer.Deserialize<SavedPromptsData>(json);
                
                _logger.LogInformation("Loaded {Count} saved prompts", data?.SavedItems?.Count ?? 0);
                return data?.SavedItems ?? new List<SavedPrompt>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved prompts");
                return new List<SavedPrompt>();
            }
        }

        public async Task SavePromptAsync(SavedPrompt prompt)
        {
            try
            {
                var prompts = await GetSavedPromptsAsync();
                
                // Check if updating existing prompt
                var existing = prompts.FirstOrDefault(p => p.Id == prompt.Id);
                if (existing != null)
                {
                    prompts.Remove(existing);
                }
                
                prompts.Add(prompt);

                var data = new SavedPromptsData { SavedItems = prompts };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_filePath, json);
                _logger.LogInformation("Saved prompt: {Name}", prompt.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving prompt");
                throw;
            }
        }

        public async Task DeletePromptAsync(string id)
        {
            try
            {
                var prompts = await GetSavedPromptsAsync();
                var prompt = prompts.FirstOrDefault(p => p.Id == id);
                
                if (prompt != null)
                {
                    prompts.Remove(prompt);
                    
                    var data = new SavedPromptsData { SavedItems = prompts };
                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    
                    await File.WriteAllTextAsync(_filePath, json);
                    _logger.LogInformation("Deleted prompt: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prompt");
                throw;
            }
        }
    }
}
