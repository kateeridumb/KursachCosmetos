namespace CosmeticShopWeb.Models
{
    public class UserViewModel
    {
        public int Id_User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RoleUs { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}