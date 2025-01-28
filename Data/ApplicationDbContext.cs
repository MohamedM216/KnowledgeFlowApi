using KnowledgeFlowApi.Entities;
using KnowledgeFlowApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeFlowApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<FileItem> FileItems { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<UserProfileImage> UserProfileImages { get; set; }
        public DbSet<CoverImage> CoverImages { get; set; }

        public DbSet<FileRating> FileRatings { get; set; }
        public DbSet<UserRating> UserRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User - UserRefreshToken (One-to-Many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRefreshTokens)
                .WithOne(urt => urt.User)
                .HasForeignKey(urt => urt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - UserProfileImage (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserProfileImage)
                .WithOne(upi => upi.User)
                .HasForeignKey<UserProfileImage>(upi => upi.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FileItem - CoverImage (One-to-One)
            modelBuilder.Entity<FileItem>()
                .HasOne(fi => fi.CoverImage)
                .WithOne(ci => ci.FileItem)
                .HasForeignKey<CoverImage>(ci => ci.FileItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.FileItems)
                .WithOne(fi => fi.User)
                .HasForeignKey(fi => fi.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // User-FileRating (One-to-Many)
            modelBuilder.Entity<FileRating>()
                .HasOne(fr => fr.User)  // A rating belongs to a user
                .WithMany(u => u.FileRatings) // A user can give multiple file ratings
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Or DeleteBehavior.SetNull

            // FileItem-FileRating (One-to-Many)
            modelBuilder.Entity<FileRating>()
                .HasOne(fr => fr.FileItem) // A rating is for a file
                .WithMany(f => f.FileRatings) // A file can have multiple ratings
                .HasForeignKey(fr => fr.FileItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.GivenUserRatings)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Change to NoAction

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.RatedUser)
                .WithMany(u => u.ReceivedUserRatings)
                .HasForeignKey(ur => ur.RatedUserId)
                .OnDelete(DeleteBehavior.NoAction); // Change to NoAction


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<FileItem>()       
                .HasIndex(p => p.Title)
                .IsUnique();

            modelBuilder.Entity<FileRating>()
                .HasIndex(fr => fr.UserId);

            modelBuilder.Entity<FileRating>()
                .HasIndex(fr => fr.FileItemId);

            modelBuilder.Entity<UserRating>()
                .HasIndex(ur => ur.UserId);

            modelBuilder.Entity<UserRating>()
                .HasIndex(ur => ur.RatedUserId);
            
            modelBuilder.Entity<FileItem>()
                .Property(f => f.TotalRating)
                .HasPrecision(18, 2); // Example: 18 total digits, 2 decimal places

            modelBuilder.Entity<FileRating>()
                .Property(fr => fr.Value)
                .HasPrecision(18, 2); // Example: 18 total digits, 2 decimal places

            modelBuilder.Entity<UserRating>()
                .Property(ur => ur.Value)
                .HasPrecision(18, 2); // Example: 18 total digits, 2 decimal places

            modelBuilder.Entity<User>()
                .Property(ur => ur.TotalRating)
                .HasPrecision(18, 2);
        }
    }
}