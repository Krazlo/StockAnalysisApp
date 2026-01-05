using UIApplication.DTO;

namespace UIApplication.Services
{
    public interface IApiService
    {
        Task<string> AnalyzeStockAsync(string prompt, string symbol, string exchange);
        Task<string> AnalyzeStockImageAsync(string prompt, string base64Image);
        Task<string> AnalyzeStockImagesAsync(string prompt, List<ImageInputDto> images);


    }
}
