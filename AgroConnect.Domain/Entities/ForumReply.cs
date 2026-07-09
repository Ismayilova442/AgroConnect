namespace AgroConnect.Domain.Entities
{
    public class ForumReply
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Is it the best answer? (StackOverflow style)
        public bool IsBestAnswer { get; set; } = false;

        public int ForumTopicId { get; set; }
        public ForumTopic? ForumTopic { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
