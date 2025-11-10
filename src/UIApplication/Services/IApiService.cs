namespace UIApplication.Services
{
    public interface IApiService
    {
        Task<string> AnalyzeStockAsync(string prompt, string symbol);
    }
}
