using System.ComponentModel.DataAnnotations;

namespace CosmeticShopAPI.DTOs
{
    public class ReviewDTO
    {
        public int IdReview { get; set; }

        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string CommentRe { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; }
    }
}