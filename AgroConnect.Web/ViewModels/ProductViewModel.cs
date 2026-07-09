using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AgroConnect.Web.ViewModels
{
    public class ProductViewModel
    {
        [Required(ErrorMessage = "Məhsulun adı tələb olunur")]
        [Display(Name = "Məhsul Adı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Məhsul haqqında məlumat yazın")]
        [Display(Name = "Təsvir")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Qiymət tələb olunur")]
        [Range(0.01, 100000, ErrorMessage = "Düzgün qiymət daxil edin")]
        [Display(Name = "Qiymət (AZN)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miqdarı tələb olunur")]
        [Range(1, 100000, ErrorMessage = "Ən azı 1 ədəd (və ya kq) olmalıdır")]
        [Display(Name = "Miqdar (Stok)")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Kateqoriya seçilməlidir")]
        [Display(Name = "Kateqoriya")]
        public int CategoryId { get; set; }

        [Display(Name = "Məhsulun Şəkli")]
        public IFormFile? Image { get; set; }
    }
}
