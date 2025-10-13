using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class ProfileViewModel
    {
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Роль")]
        public string RoleUs { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime? DateRegistered { get; set; }

        [Display(Name = "Статус")]
        public string StatusUs { get; set; }
    }
}