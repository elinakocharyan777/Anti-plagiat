using System.Net.Http.Json;
using Shared.Models;

namespace GatewayService.Clients;

public class AnalysisClient : IAnalysisClient
{
    private readonly HttpClient _httpClient;

    public AnalysisClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SubmissionSummaryDto> CreateSubmissionAsync(
        CreateSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/submissions", request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"AnalysisService returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

        var dto = await response.Content.ReadFromJsonAsync<SubmissionSummaryDto>(cancellationToken: cancellationToken);
        return dto ?? throw new InvalidOperationException("Empty response from AnalysisService");
    }

    public async Task<IReadOnlyList<SubmissionSummaryDto>> GetByAssignmentAsync(
        string assignmentId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/submissions/by-assignment/{assignmentId}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"AnalysisService returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

        var list = await response.Content.ReadFromJsonAsync<List<SubmissionSummaryDto>>(cancellationToken: cancellationToken);
        return list ?? new List<SubmissionSummaryDto>();
    }

    public async Task<SubmissionReportDto> GetReportAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/submissions/{submissionId}/report",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"AnalysisService returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

        var dto = await response.Content.ReadFromJsonAsync<SubmissionReportDto>(cancellationToken: cancellationToken);
        return dto ?? throw new InvalidOperationException("Empty response from AnalysisService");
    }
}
