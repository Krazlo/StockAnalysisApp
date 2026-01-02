using AIAnalysisService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class AIAnalysisServiceFactory
    : WebApplicationFactory<AIAnalysisService.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IGeminiService)
            );

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddScoped<IGeminiService, FakeGeminiService>();
        });
    }
}