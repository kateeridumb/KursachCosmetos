using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class CheckoutViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Город обязателен")]
        public string City { get; set; }

        [Required(ErrorMessage = "Адрес обязателен")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Почтовый индекс обязателен")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Выберите способ доставки")]
        public string DeliveryMethod { get; set; }

        [Required(ErrorMessage = "Выберите способ оплаты")]
        public string PaymentMethod { get; set; }

        public string Comment { get; set; }

        public CartViewModel Cart { get; set; }

        public decimal DeliveryCost { get; set; }
        public decimal TotalAmount => (Cart?.TotalPrice ?? 0) + DeliveryCost;
    }
}
