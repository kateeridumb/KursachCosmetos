namespace CosmeticShopWeb.Models
{
    public class ApiUserResponse
    {
        public int IdUser { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleUs { get; set; }
        public DateTime? DateRegistered { get; set; }
        public string StatusUs { get; set; }
    }
}
