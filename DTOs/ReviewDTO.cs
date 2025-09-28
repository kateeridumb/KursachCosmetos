using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class ReviewDTO
    {
        public int IdReview { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? CommentRe { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
