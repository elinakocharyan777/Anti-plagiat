using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnalysisService.Models
{
    public class Report
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SubmissionId { get; set; }

        [ForeignKey(nameof(SubmissionId))]
        public Submission Submission { get; set; } = null!;

        public bool IsPlagiarism { get; set; }
        public Guid? OriginalSubmissionId { get; set; }

        public string? DetailsJson { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
