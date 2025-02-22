using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeFlowApi.Entities
{
    
    public class FileRating
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        [ForeignKey("FileItem")]
        public int FileItemId { get; set; }
        public FileItem FileItem { get; set; }

        public decimal Value { get; set; } // from 1 to 5
        public string Review { get; set; }
        public DateTime RatedOn { get; set; }
    }
}