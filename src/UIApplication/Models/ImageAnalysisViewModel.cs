namespace UIApplication.Models
{
    public class ImageAnalysisViewModel
    {
        public string Prompt { get; set; } = string.Empty;

        public List<IFormFile>? Images { get; set; }

        public string? AnalysisResult { get; set; }

        public string? ErrorMessage { get; set; }

        public bool IsLoading { get; set; }
    }
}
