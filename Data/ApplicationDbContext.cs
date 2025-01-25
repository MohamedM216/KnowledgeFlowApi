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

        // public DbSet<FileRating> FileRatings { get; set; }

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

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<FileItem>()       
                .HasIndex(p => p.Title)
                .IsUnique();
        }
    }
}