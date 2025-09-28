using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
