using AIAnalysisService.Models;

namespace AIAnalysisService.Services
{
    public interface IGeminiService
    {
        Task<AnalysisResponse> AnalyzeStockAsync(AnalysisRequest request);
    }
}
