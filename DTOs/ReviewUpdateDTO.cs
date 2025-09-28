namespace CosmeticShopAPI.DTOs
{
    public class ReviewUpdateDTO
    {
        public int IdReview { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? CommentRe { get; set; }
    }
}
