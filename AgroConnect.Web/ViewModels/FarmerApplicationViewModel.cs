using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AgroConnect.Web.ViewModels
{
    public class FarmerApplicationViewModel
    {
        [Required(ErrorMessage = "Rayon seçilməlidir")]
        [Display(Name = "Rayon")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kəndin adı qeyd olunmalıdır")]
        [Display(Name = "Kənd")]
        public string Village { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fermerlik növü seçilməlidir (məs: Arıçılıq, Maldarlıq)")]
        [Display(Name = "Fermerlik növü")]
        public string FarmType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Təcrübə (il) qeyd olunmalıdır")]
        [Range(0, 100, ErrorMessage = "Düzgün təcrübə ili daxil edin")]
        [Display(Name = "Neçə ildir fəaliyyət göstərirsiniz?")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Şəxsiyyət vəsiqəsinin şəkli mütləqdir")]
        [Display(Name = "Şəxsiyyət vəsiqəsinin şəkli (Ön və Arxa birgə və ya sadəcə Ön)")]
        public IFormFile? IDCardImage { get; set; }

        [Required(ErrorMessage = "Fermanın (və ya təsərrüfatın) şəkli mütləqdir")]
        [Display(Name = "Təsərrüfatınızın şəkli")]
        public IFormFile? FarmImage { get; set; }

        [Required(ErrorMessage = "Özünüz və təsərrüfatınız haqqında qısa məlumat verin")]
        [Display(Name = "Qısa məlumat")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Məlumat ən azı 10 simvol olmalıdır")]
        public string About { get; set; } = string.Empty;
    }
}
