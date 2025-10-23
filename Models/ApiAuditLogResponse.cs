namespace CosmeticShopWeb.Models
{
    public class ApiAuditLogResponse
    {
        public int Id_Log { get; set; }
        public int? UserID { get; set; }
        public string UserName { get; set; }
        public string TableName { get; set; }
        public string ActionType { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public DateTime TimestampMl { get; set; }
    }

}
