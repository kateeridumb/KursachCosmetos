namespace CosmeticShopWeb.Models
{
    public class AdminPanelViewModel
    {
        public List<AuditLogViewModel> AuditLogs { get; set; } = new List<AuditLogViewModel>();
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public List<AdminOrderViewModel> Orders { get; set; } = new List<AdminOrderViewModel>();
        public string ActiveTab { get; set; } = "logs";
    }
}
