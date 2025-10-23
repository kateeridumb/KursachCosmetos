using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class RegisterEmployeeViewModel
    {
        [Required(ErrorMessage = "Фамилия обязательна")]
        [RegularExpression(@"^[А-ЯЁа-яё\s'-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробелы и дефисы")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Имя обязательно")]
        [RegularExpression(@"^[А-ЯЁа-яё\s'-]+$", ErrorMessage = "Имя может содержать только буквы, пробелы и дефисы")]
        public string FirstName { get; set; } = null!;

        [RegularExpression(@"^[А-ЯЁа-яё\s'-]*$", ErrorMessage = "Отчество может содержать только буквы, пробелы и дефисы")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Телефон обязателен")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Телефон должен содержать ровно 11 цифр")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Роль обязательна")]
        public string RoleUs { get; set; } = "Менеджер";

        public string? Gender { get; set; }
    }
}