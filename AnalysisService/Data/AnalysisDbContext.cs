using AnalysisService.Models;
using Microsoft.EntityFrameworkCore;

namespace AnalysisService.Data
{
    public class AnalysisDbContext : DbContext
    {
        public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options)
            : base(options)
        {
        }

        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<Report> Reports => Set<Report>();
    }
}
