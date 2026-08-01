using System.ComponentModel.DataAnnotations;

namespace AgroConnect.Web.ViewModels
{
    public class ComplaintViewModel
    {
        [Required(ErrorMessage = "Mövzu tələb olunur")]
        [StringLength(150, ErrorMessage = "Mövzu maksimum 150 simvol ola bilər")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şikayət mətni tələb olunur")]
        [StringLength(2000, ErrorMessage = "Mətn maksimum 2000 simvol ola bilər")]
        public string Content { get; set; } = string.Empty;
    }
}