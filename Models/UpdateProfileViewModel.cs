using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        [StringLength(50, ErrorMessage = "Фамилия не может быть длиннее 50 символов")]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        [StringLength(50, ErrorMessage = "Отчество не может быть длиннее 50 символов")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Display(Name = "Новый пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        public string NewPassword { get; set; }

        [Display(Name = "Подтверждение пароля")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}