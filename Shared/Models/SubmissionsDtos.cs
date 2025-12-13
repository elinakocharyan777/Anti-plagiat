namespace Shared.Models
{
    public class CreateSubmissionRequest
    {
        public string StudentId { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public Guid FileId { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
    }

    public class SubmissionSummaryDto
    {
        public Guid SubmissionId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public DateTime SubmittedAtUtc { get; set; }
        public string Status { get; set; } = string.Empty; 
        public bool? IsPlagiarism { get; set; }
        public Guid? OriginalSubmissionId { get; set; }
    }

    public class SubmissionReportDto
    {
        public Guid SubmissionId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public DateTime SubmittedAtUtc { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPlagiarism { get; set; }
        public Guid? OriginalSubmissionId { get; set; }
        public string? DetailsJson { get; set; }
    }
}
