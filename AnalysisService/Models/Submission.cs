using System;
using System.ComponentModel.DataAnnotations;

namespace AnalysisService.Models
{
    public class Submission
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string AssignmentId { get; set; } = string.Empty;

        [Required]
        public Guid FileId { get; set; }   

        public DateTime SubmittedAtUtc { get; set; }

        [MaxLength(16)]
        public string Status { get; set; } = "DONE";

      
        public string? FileHash { get; set; }
    }
}
