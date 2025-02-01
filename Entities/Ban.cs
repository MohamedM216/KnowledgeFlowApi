namespace KnowledgeFlowApi.Entities
{
    public class Ban
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime BanStartDate { get; set; }
        public DateTime? BanEndDate { get; set; } // Nullable for permanent bans
        public string Reason { get; set; }

        public User User { get; set; }
    }
}