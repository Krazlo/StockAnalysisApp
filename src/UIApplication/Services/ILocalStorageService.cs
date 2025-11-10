using UIApplication.Models;

namespace UIApplication.Services
{
    public interface ILocalStorageService
    {
        Task<List<SavedPrompt>> GetSavedPromptsAsync();
        Task SavePromptAsync(SavedPrompt prompt);
        Task DeletePromptAsync(string id);
    }
}
