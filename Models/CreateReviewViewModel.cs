using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class CreateReviewViewModel
    {
        [Required(ErrorMessage = "ID продукта обязателен")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Оценка обязательна")]
        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Отзыв не может быть пустым")]
        [StringLength(1000, ErrorMessage = "Отзыв не должен превышать 1000 символов")]
        public string CommentRe { get; set; }
    }
}