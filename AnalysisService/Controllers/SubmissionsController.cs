using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnalysisService.Data;
using AnalysisService.DTO;
using AnalysisService.Models;
using AnalysisService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnalysisService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly AnalysisDbContext _db;
        private readonly IPlagiarismAnalyzer _analyzer;

        public SubmissionsController(AnalysisDbContext db, IPlagiarismAnalyzer analyzer)
        {
            _db = db;
            _analyzer = analyzer;
        }

        [HttpPost]
        public async Task<ActionResult<SubmissionSummaryDto>> Create(
            [FromBody] CreateSubmissionRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.StudentId) ||
                string.IsNullOrWhiteSpace(request.AssignmentId) ||
                request.FileId == Guid.Empty)
            {
                return BadRequest(new { error = "studentId, assignmentId и fileId обязательны" });
            }

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                AssignmentId = request.AssignmentId,
                FileId = request.FileId,
                SubmittedAtUtc = DateTime.UtcNow,
                Status = "NEW"
            };

            _db.Submissions.Add(submission);

            // анализ плагиата по FileId
            var report = await _analyzer.AnalyzeAsync(submission, cancellationToken);
            _db.Reports.Add(report);

            await _db.SaveChangesAsync(cancellationToken);

            var dto = new SubmissionSummaryDto
            {
                SubmissionId = submission.Id,
                StudentId = submission.StudentId,
                AssignmentId = submission.AssignmentId,
                SubmittedAtUtc = submission.SubmittedAtUtc,
                Status = submission.Status,
                IsPlagiarism = report.IsPlagiarism,
                OriginalSubmissionId = report.OriginalSubmissionId
            };

            return Ok(dto);
        }

        [HttpGet("by-assignment/{assignmentId}")]
        public async Task<ActionResult<IEnumerable<SubmissionSummaryDto>>> GetByAssignment(
            string assignmentId,
            CancellationToken cancellationToken)
        {
            var submissions = await _db.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .OrderBy(s => s.SubmittedAtUtc)
                .ToListAsync(cancellationToken);

            var submissionIds = submissions.Select(s => s.Id).ToList();

            var reports = await _db.Reports
                .Where(r => submissionIds.Contains(r.SubmissionId))
                .ToListAsync(cancellationToken);

            var result = from s in submissions
                         join r in reports on s.Id equals r.SubmissionId into gj
                         from r in gj.DefaultIfEmpty()
                         select new SubmissionSummaryDto
                         {
                             SubmissionId = s.Id,
                             StudentId = s.StudentId,
                             AssignmentId = s.AssignmentId,
                             SubmittedAtUtc = s.SubmittedAtUtc,
                             Status = s.Status,
                             IsPlagiarism = r?.IsPlagiarism,
                             OriginalSubmissionId = r?.OriginalSubmissionId
                         };

            return Ok(result.ToList());
        }

        [HttpGet("{id}/report")]
        public async Task<ActionResult<SubmissionReportDto>> GetReport(
            Guid id,
            CancellationToken cancellationToken)
        {
            var submission = await _db.Submissions
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (submission == null)
                return NotFound();

            var report = await _db.Reports
                .FirstOrDefaultAsync(r => r.SubmissionId == submission.Id, cancellationToken);

            if (report == null)
                return NotFound();

            var dto = new SubmissionReportDto
            {
                SubmissionId = submission.Id,
                StudentId = submission.StudentId,
                AssignmentId = submission.AssignmentId,
                SubmittedAtUtc = submission.SubmittedAtUtc,
                Status = submission.Status,
                IsPlagiarism = report.IsPlagiarism,
                OriginalSubmissionId = report.OriginalSubmissionId,
                DetailsJson = report.DetailsJson
            };

            return Ok(dto);
        }
    }
}
