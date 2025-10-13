using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "Выберите способ оплаты")]
        public string PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; }
        public string OrderData { get; set; }

        [Required(ErrorMessage = "Номер карты обязателен")]
        [RegularExpression(@"^[0-9\s]{16,22}$", ErrorMessage = "Номер карты должен содержать 16-19 цифр")]
        [Display(Name = "Номер карты")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Срок действия обязателен")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([2-9][0-9])$", ErrorMessage = "Формат: ММ/ГГ (например: 12/25)")]
        [Display(Name = "Срок действия")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV обязателен")]
        [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "CVV должен содержать 3 цифры")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен")]
        [RegularExpression(@"^\+7\s?[0-9]{3}\s?[0-9]{3}\s?[0-9]{2}\s?[0-9]{2}$",
                         ErrorMessage = "Формат: +7 999 999 99 99")]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }
    }
}