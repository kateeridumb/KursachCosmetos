using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Токен обязателен")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Пароль должен содержать минимум 12 символов")]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).+$",
            ErrorMessage = "Пароль должен содержать минимум 1 цифру и 1 специальный символ")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = null!;
    }
}