using CheckCarsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Data
{
    /// <summary>
    /// Represents the database context for managing reports and related entities.
    /// </summary>
    public class ReportsDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the DbSet of bookings.
        /// </summary>
        public DbSet<Booking> Bookings { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of crash reports.
        /// </summary>
        public DbSet<CrashReport> CrashReports { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of entry and exit reports.
        /// </summary>
        public DbSet<EntryExitReport> EntryExitReports { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of vehicle return reports.
        /// </summary>
        public DbSet<VehicleReturn> VehicleReturns { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of issue reports.
        /// </summary>
        public DbSet<IssueReport> IssueReports { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of photos.
        /// </summary>
        public DbSet<Photo> Photos { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of cars.
        /// </summary>
        public DbSet<Car> Cars { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of car services.
        /// </summary>
        public DbSet<CarService> CarsService { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of commentaries.
        /// </summary>
        public DbSet<Commentary> commentaries { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of reminders.
        /// </summary>
        public DbSet<Reminder> Reminders { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of reminder destinations.
        /// </summary>
        public DbSet<ReminderDest> ReminderDests { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of vehicle attachments.
        /// </summary>
        public DbSet<VehicleAttachment> VehicleAttachments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public ReportsDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Configures the model creating for the DbContext, using Table-per-Concrete class strategy for Reports.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>().UseTpcMappingStrategy();

            base.OnModelCreating(modelBuilder);
        }
    }
}
