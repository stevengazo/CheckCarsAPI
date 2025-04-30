using CheckCarsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Data
{
    public class ReportsDbContext : DbContext
    {
        public DbSet<CrashReport> CrashReports { get; set; }
        public DbSet<EntryExitReport> EntryExitReports { get; set; }
        public DbSet<IssueReport> IssueReports { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarService> CarsService { get; set; }
        public DbSet<Commentary> commentaries { get; set; }
        public DbSet<Reminder> Reminders {get;set;}
        public DbSet<ReminderDest> ReminderDests {get;set;}
        public DbSet<VehicleAttachment> VehicleAttachments { get; set; }    
        
        public ReportsDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>().UseTpcMappingStrategy();

            base.OnModelCreating(modelBuilder);
        }
    }
}
