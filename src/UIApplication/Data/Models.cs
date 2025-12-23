using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace UIApplication.Data
{
    // Custom model for user-defined prompts
    public class UserPrompt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; }

        [Required]
        public string Prompt { get; set; } = string.Empty;
        [Required]
        public string Symbol { get; set; } = string.Empty;
        [Required]
        public string Exchange { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }

    // Custom model for storing analysis results
    public class StockAnalysis
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; }

        [Required]
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string AnalysisResult { get; set; } // The text result from AIAnalysisService

        public string KeyIndicators { get; set; } // Summary of indicators for the graph page

        public int? UserPromptId { get; set; }

        [ForeignKey("UserPromptId")]
        public UserPrompt? UsedPrompt { get; set; }
    }
}
