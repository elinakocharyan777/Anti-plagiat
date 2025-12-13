using Shared.Models;

namespace GatewayService.Clients;

public interface IAnalysisClient
{
    Task<SubmissionSummaryDto> CreateSubmissionAsync(CreateSubmissionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubmissionSummaryDto>> GetByAssignmentAsync(string assignmentId, CancellationToken cancellationToken = default);
    Task<SubmissionReportDto> GetReportAsync(Guid submissionId, CancellationToken cancellationToken = default);
}
