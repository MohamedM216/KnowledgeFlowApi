namespace KnowledgeFlowApi.DTOs
{
    public class AddCommentDto
    {
        public int FileId { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
        public string comment { get; set; }

    }
}