using AIAnalysisService.Models;

namespace AIAnalysisService.Services
{
    public interface IGeminiService
    {
        Task<AnalysisResponse> AnalyzeStockAsync(AnalysisRequest request);
        Task<string> AnalyzeImage(string prompt, List<ImageInput> images);

    }
}
