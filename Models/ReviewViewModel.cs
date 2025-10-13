using System;
using System.ComponentModel.DataAnnotations;

namespace CosmeticShopWeb.Models
{
    public class ReviewViewModel
    {
        public int IdReview { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Рейтинг обязателен")]
        [Range(1, 5, ErrorMessage = "Рейтинг должен быть от 1 до 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Отзыв не может быть пустым")]
        [StringLength(1000, ErrorMessage = "Отзыв не может превышать 1000 символов")]
        public string CommentRe { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FormattedDate => CreatedAt.ToString("dd.MM.yyyy");
    }
}