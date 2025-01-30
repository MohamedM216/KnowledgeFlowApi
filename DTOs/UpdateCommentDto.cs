using System.ComponentModel.DataAnnotations;

namespace KnowledgeFlowApi.DTOs
{
    public class UpdateCommentDto
    {   
        [Required]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
    }
}