using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Фамилия обязательна")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Имя обязательно")]
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Роль обязательна")]
        public string RoleUs { get; set; } = "Клиент"; 

        public string StatusUs { get; set; } = "Активен"; 

        public string? Gender { get; set; }
    }
}
