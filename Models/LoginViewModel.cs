using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; }

        public bool RememberMe { get; set; } 
    }
}