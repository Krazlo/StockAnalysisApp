using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using UIApplication.Data;
using UIApplication.Models;

namespace UIApplication.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly ApplicationDbContext _context;

        public AnalysisService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserPrompt>> GetUserPromptsAsync(string userId)
        {
            return await _context.UserPrompts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task SaveUserPromptAsync(string userId, string title, string symbol, string exchange, string prompt)
        {
            var userPrompt = new UserPrompt
            {
                UserId = userId,
                Title = title,
                Symbol = symbol,
                Exchange = exchange,
                Prompt = prompt
            };
            _context.UserPrompts.Add(userPrompt);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserPromptAsync(string userId, int promptId)
        {
            var prompt = await _context.UserPrompts
                .FirstOrDefaultAsync(p => p.Id == promptId && p.UserId == userId);

            if (prompt != null)
            {
                _context.UserPrompts.Remove(prompt);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserPrompt?> GetUserPromptByIdAsync(int promptId)
        {
            return await _context.UserPrompts.FindAsync(promptId);
        }

        public async Task SaveStockAnalysisAsync(string userId, string symbol, string analysisResult, string keyIndicators, int? userPromptId)
        {
            var analysis = new StockAnalysis
            {
                UserId = userId,
                Symbol = symbol,
                AnalysisResult = analysisResult,
                KeyIndicators = keyIndicators,
                UserPromptId = userPromptId
            };
            _context.StockAnalyses.Add(analysis);
            await _context.SaveChangesAsync();
        }

        public async Task<List<StockAnalysis>> GetUserAnalysesAsync(string userId)
        {
            return await _context.StockAnalyses
                .Where(a => a.UserId == userId)
                .Include(a => a.UsedPrompt)
                .OrderByDescending(a => a.AnalysisDate)
                .ToListAsync();
        }
    }
}
