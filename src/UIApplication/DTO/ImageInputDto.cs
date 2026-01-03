namespace UIApplication.DTO
{
    public class ImageInputDto
    {
        public string Base64 { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
