namespace AIAnalysisService.DTO
{
    public class AnalysisResponseDto
    {
        public string Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Analysis { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
