using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeFlowApi.Entities
{
    public class UserRating
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        
        [ForeignKey("RatedUser")]
        public int RatedUserId { get; set; }
        public User RatedUser { get; set; }

        public decimal Value { get; set; } // from 1 to 5
        public DateTime RatedOn { get; set; }
    }
}