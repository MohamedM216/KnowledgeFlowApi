namespace KnowledgeFlowApi.Entities
{
    // user x rates pdf y with value z
    public class FileRating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int FileItemId { get; set; }
        public FileItem FileItem { get; set; }
        public int Value { get; set; } // from 1 to 5
    }
}