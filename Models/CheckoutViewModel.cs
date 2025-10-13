using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Город обязателен")]
        [Display(Name = "Город")]
        public string City { get; set; }

        [Required(ErrorMessage = "Адрес обязателен")]
        [Display(Name = "Адрес")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Почтовый индекс обязателен")]
        [Display(Name = "Почтовый индекс")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Выберите способ доставки")]
        [Display(Name = "Способ доставки")]
        public string DeliveryMethod { get; set; }

        [Required(ErrorMessage = "Выберите способ оплаты")]
        [Display(Name = "Способ оплаты")]
        public string PaymentMethod { get; set; }

        public string Comment { get; set; }

        public CartViewModel Cart { get; set; }
        public decimal DeliveryCost { get; set; }
        public decimal TotalAmount => (Cart?.TotalPrice ?? 0) + DeliveryCost;
    }
}
