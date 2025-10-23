namespace CosmeticShopWeb.Models
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string TableName { get; set; }
        public string ActionType { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
