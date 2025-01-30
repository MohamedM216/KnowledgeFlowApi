using KnowledgeFlowApi.Entities;

namespace KnowledgeFlowApi.DTOs
{
    public class GetCommentDto
    {
        public bool IsCreated = false;
        public string? Message { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
        public string comment { get; set; }
        public List<GetCommentDto>? Replies { get; set; }

    }
}