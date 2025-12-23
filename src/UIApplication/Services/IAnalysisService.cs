using UIApplication.Data;
using UIApplication.Models;

namespace UIApplication.Services
{
    public interface IAnalysisService
    {
        Task<List<UserPrompt>> GetUserPromptsAsync(string userId);
        Task SaveUserPromptAsync(string userId, string title, string symbol, string exchange, string prompt);
        Task DeleteUserPromptAsync(string userId, int promptId);
        Task<UserPrompt?> GetUserPromptByIdAsync(int promptId);
        Task SaveStockAnalysisAsync(string userId, string symbol, string analysisResult, string keyIndicators, int? userPromptId);
        Task<List<StockAnalysis>> GetUserAnalysesAsync(string userId);
    }
}
