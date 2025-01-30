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
        public DbSet<Comment> Comments { get; set; }

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
                .HasOne(fr => fr.User)  
                .WithMany(u => u.FileRatings) 
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // FileItem-FileRating (One-to-Many)
            modelBuilder.Entity<FileRating>()
                .HasOne(fr => fr.FileItem) 
                .WithMany(f => f.FileRatings)
                .HasForeignKey(fr => fr.FileItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.GivenUserRatings)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.RatedUser)
                .WithMany(u => u.ReceivedUserRatings)
                .HasForeignKey(ur => ur.RatedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // relationship between File and Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.FileItem) 
                .WithMany(f => f.Comments) 
                .HasForeignKey(c => c.FileItemId) 
                .OnDelete(DeleteBehavior.Cascade);

            // relationship between User and Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User) 
                .WithMany(u => u.Comments) 
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if they have comments

            //  self-referencing relationship for Comment Replies
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment) 
                .WithMany(c => c.Replies) 
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if there are replies


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