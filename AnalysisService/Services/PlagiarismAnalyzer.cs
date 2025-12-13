using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnalysisService.Data;
using AnalysisService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AnalysisService.Services
{
  
    public class PlagiarismAnalyzer : IPlagiarismAnalyzer
    {
        private readonly AnalysisDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PlagiarismAnalyzer(
            AnalysisDbContext db,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<Report> AnalyzeAsync(
            Submission submission,
            CancellationToken cancellationToken = default)
        {
            string fileHash;
            try
            {
                fileHash = await ComputeFileHashAsync(submission.FileId, cancellationToken);
            }
            catch (Exception ex)
            {
                fileHash = submission.FileId.ToString();
            }

            submission.FileHash = fileHash;

            var original = await _db.Submissions
                .Where(s =>
                    s.AssignmentId == submission.AssignmentId &&
                    s.FileHash == fileHash &&
                    s.StudentId != submission.StudentId &&
                    s.Id != submission.Id && 
                    s.SubmittedAtUtc <= submission.SubmittedAtUtc) 
                .OrderBy(s => s.SubmittedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            var report = new Report
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                Submission = submission,
                IsPlagiarism = original != null,
                OriginalSubmissionId = original?.Id,
                DetailsJson = null,
                CreatedAtUtc = DateTime.UtcNow
            };

            submission.Status = "DONE";

            return report;
        }

        private async Task<string> ComputeFileHashAsync(Guid fileId, CancellationToken cancellationToken)
        {
            var fileStorageBaseUrl = _configuration["FileStorage:BaseUrl"]
                ?? throw new InvalidOperationException("FileStorage:BaseUrl not configured");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(fileStorageBaseUrl);

            var response = await client.GetAsync($"/api/Files/{fileId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return hashString;
        }
    }
}
