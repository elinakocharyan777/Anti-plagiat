using System.Threading;
using System.Threading.Tasks;
using AnalysisService.Models;

namespace AnalysisService.Services
{
    public interface IPlagiarismAnalyzer
    {
        Task<Report> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default);
    }
}
