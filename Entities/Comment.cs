using System.ComponentModel.DataAnnotations;

namespace KnowledgeFlowApi.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime CreatedOn {get; set; }
        public int? ParentCommentId { get; set; }  // null for top-level comments
        public int FileItemId { get; set; }
        public int UserId { get; set; }

        public Comment ParentComment { get; set; }
        public User User { get; set; }
        public FileItem FileItem { get; set; }
        public ICollection<Comment> Replies { get; set; }
        // Use optimistic concurrency control by adding a RowVersion column
        //  to the Comment entity and handling concurrency exceptions.
    //         The [Timestamp] attribute tells Entity Framework Core to treat this property as a concurrency token.

    // byte[] is the recommended type for RowVersion because it maps to SQL Server's rowversion (or timestamp) data type.
        [Timestamp]
        public byte[] RowVersion { get; set; } // Add this for concurrency control
    }
}