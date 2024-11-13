using CheckCars.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Data
{
    public class ReportsDbContext : DbContext
    {
        DbSet<CrashReport> CrashReports {  get; set; }
        DbSet<EntryExitReport> EntryExitReports { get; set; }
        DbSet<IssueReport> IssueReports { get; set; }
        DbSet<Photo> Photos { get; set; }

        public ReportsDbContext(DbContextOptions options) : base( options)
        {
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>().UseTpcMappingStrategy();

            base.OnModelCreating(modelBuilder);
        }
    }
}
