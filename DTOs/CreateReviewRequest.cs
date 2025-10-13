namespace CosmeticShopAPI.DTOs
{

    public class CreateReviewRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string CommentRe { get; set; }
    }
}
