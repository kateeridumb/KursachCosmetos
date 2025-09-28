using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticShopAPI.DTOs
{
    public class AuditLogDTO
    {
        public int IdLog { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string TableName { get; set; } = null!;
        public string ActionType { get; set; } = null!;
        public string? OldData { get; set; }
        public string? NewData { get; set; }
        public DateTime TimestampMl { get; set; }
    }

}
