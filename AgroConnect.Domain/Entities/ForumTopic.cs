namespace AgroConnect.Domain.Entities
{
    public class ForumTopic
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ViewsCount { get; set; } = 0;

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        public ICollection<ForumReply> Replies { get; set; } = new List<ForumReply>();
    }
}
