namespace CosmeticShopWeb.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleUs { get; set; }
        public string Status { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }
}