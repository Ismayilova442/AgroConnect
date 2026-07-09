using System.ComponentModel.DataAnnotations;

namespace AgroConnect.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad tələb olunur")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad tələb olunur")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Düzgün email ünvanı daxil edin")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə tələb olunur")]
        [StringLength(100, ErrorMessage = "{0} ən azı {2} simvol olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifrə (Təkrar)")]
        [Compare("Password", ErrorMessage = "Şifrələr uyğun gəlmir.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə tələb olunur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Məni xatırla?")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə tələb olunur")]
        [StringLength(100, ErrorMessage = "{0} ən azı {2} simvol olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifrə")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifrə (Təkrar)")]
        [Compare("Password", ErrorMessage = "Şifrələr uyğun gəlmir.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
