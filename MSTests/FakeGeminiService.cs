using AIAnalysisService.Models;
using AIAnalysisService.Services;

public class FakeGeminiService : IGeminiService
{
    public Task<AnalysisResponse> AnalyzeStockAsync(AnalysisRequest request)
    {
        AnalysisResponse result = new AnalysisResponse()
        {
            Symbol = request.Symbol,
            Analysis = "Mega god analyse",
            Timestamp = DateTime.Now,
            Success = true
        };

        return Task.FromResult(result);
    }

    public Task<string> AnalyzeImage(string prompt, List<ImageInput> images)
    {
        throw new NotImplementedException();
    }
}