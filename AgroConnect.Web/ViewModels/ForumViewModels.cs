using System.ComponentModel.DataAnnotations;

namespace AgroConnect.Web.ViewModels
{
    public class ForumTopicViewModel
    {
        [Required(ErrorMessage = "Mövzu başlığı tələb olunur")]
        [Display(Name = "Başlıq")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Məzmun tələb olunur")]
        [Display(Name = "Məzmun (Sual və ya mövzu)")]
        public string Content { get; set; } = string.Empty;
    }

    public class ForumReplyViewModel
    {
        public int TopicId { get; set; }
        
        [Required(ErrorMessage = "Cavab mətni tələb olunur")]
        public string Content { get; set; } = string.Empty;
    }
}
