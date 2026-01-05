using AIAnalysisService.Models;

namespace AIAnalysisService.DTO
{
    public class ImageAnalysisRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public List<ImageInput> Images { get; set; } = new();
    }

}
