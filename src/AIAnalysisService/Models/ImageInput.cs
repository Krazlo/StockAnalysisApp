namespace AIAnalysisService.Models
{
    public class ImageInput
    {
        public string Base64 { get; set; } = default!;
        public string MimeType { get; set; } = "image/png";
        public string? Description { get; set; }
    }
}
