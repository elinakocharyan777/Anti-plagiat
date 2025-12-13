using GatewayService.Clients;
using GatewayService.Models;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorksController : ControllerBase
{
    private readonly IFileStorageClient _fileStorageClient;
    private readonly IAnalysisClient _analysisClient;

    public WorksController(IFileStorageClient fileStorageClient, IAnalysisClient analysisClient)
    {
        _fileStorageClient = fileStorageClient;
        _analysisClient = analysisClient;
    }

    // Отправка работы на проверку: загружаем файл и создаём сдачу в AnalysisService.
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<SubmissionSummaryDto>> UploadWork(
        [FromForm] UploadWorkRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.StudentId) ||
            string.IsNullOrWhiteSpace(request.AssignmentId) ||
            request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { error = "StudentId, AssignmentId и файл обязательны" });
        }

        try
        {
            // Загружаем файл в FileStorageService
            var uploadResult = await _fileStorageClient.UploadAsync(request.File, cancellationToken);

            // Создаём сдачу в AnalysisService
            var createRequest = new CreateSubmissionRequest
            {
                StudentId = request.StudentId,
                AssignmentId = request.AssignmentId,
                FileId = uploadResult.FileId,
                SubmittedAtUtc = DateTime.UtcNow
            };

            var summary = await _analysisClient.CreateSubmissionAsync(createRequest, cancellationToken);

            return Ok(summary);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new
            {
                error = "Один из внутренних сервисов недоступен",
                details = ex.Message
            });
        }
    }

    //Аналитика по контрольной работе (по assignmentId).
    [HttpGet("{assignmentId}/reports")]
    public async Task<ActionResult<IReadOnlyList<SubmissionSummaryDto>>> GetReportsByAssignment(
        string assignmentId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assignmentId))
            return BadRequest(new { error = "assignmentId обязателен" });

        try
        {
            var summaries = await _analysisClient.GetByAssignmentAsync(assignmentId, cancellationToken);
            return Ok(summaries);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new
            {
                error = "AnalysisService недоступен",
                details = ex.Message
            });
        }
    }

   
    // Подробный отчёт по конкретной сдаче.
    [HttpGet("submissions/{submissionId:guid}/report")]
    public async Task<ActionResult<SubmissionReportDto>> GetSubmissionReport(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _analysisClient.GetReportAsync(submissionId, cancellationToken);
            return Ok(report);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new
            {
                error = "AnalysisService недоступен",
                details = ex.Message
            });
        }
    }
}
