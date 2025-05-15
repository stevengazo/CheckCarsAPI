using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Models;

namespace CheckCarsAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserApp>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relación Muchos-a-Muchos entre User y Chat
            builder.Entity<Chat>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Chats)
                .UsingEntity(j => j.ToTable("ChatUsers"));

            // Relación Uno-a-Muchos entre User y Message
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Uno-a-Muchos entre Chat y Message
            builder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId);


            List<IdentityRole> identityRole = new()
            {
                new IdentityRole
                {
                    Id = "100",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "200",
                    Name = "Manager",
                    NormalizedName = "MANAGER"
                },
                new IdentityRole{
                    Id = "300",
                    Name = "User",
                    NormalizedName = "USER"
                },
                new IdentityRole
                {
                    Id = "400",
                    Name = "Guest",
                    NormalizedName = "GUEST"
                }
            };

            builder.Entity<IdentityRole>().HasData(identityRole);   

        }
    }
}
