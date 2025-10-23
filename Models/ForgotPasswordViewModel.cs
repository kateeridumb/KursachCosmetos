using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
    }
}
