using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Models;

namespace CheckCarsAPI.Data
{
    /// <summary>
    /// Represents the application's database context for identity and chat management.
    /// Inherits from IdentityDbContext with UserApp as the user entity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<UserApp>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet of chat rooms.
        /// </summary>
        public DbSet<Chat> Chats { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of messages.
        /// </summary>
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of user notifications.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Configures the database context options.
        /// </summary>
        /// <param name="optionsBuilder">The options builder to configure.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Configures the entity relationships and seeds roles data.
        /// </summary>
        /// <param name="builder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Many-to-many relationship between User and Chat
            builder.Entity<Chat>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Chats)
                .UsingEntity(j => j.ToTable("ChatUsers"));

            // One-to-many relationship between User and Message (Sender)
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many relationship between Chat and Message
            builder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId);

            // Notifications are queried per recipient, most often filtering by read state.
            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            // Seed default roles into the database
            // ConcurrencyStamp must be fixed; otherwise EF generates a random value
            // on every model build, which never matches the migration snapshot and
            // makes Database.Migrate() fail with PendingModelChangesWarning.
            List<IdentityRole> identityRole = new()
            {
                new IdentityRole
                {
                    Id = "100",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000100"
                },
                new IdentityRole
                {
                    Id = "200",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000200"
                },
                new IdentityRole
                {
                    Id = "300",
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000300"
                },
                new IdentityRole
                {
                    Id = "400",
                    Name = "Guest",
                    NormalizedName = "GUEST",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000400"
                }
            };

            builder.Entity<IdentityRole>().HasData(identityRole);
        }
    }
}
