namespace CosmeticShopAPI.DTOs
{
    public class RegisterDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string? Phone { get; set; }

        public string RoleUs { get; set; } = "Клиент";
        public string StatusUs { get; set; } = "Активен";
        public string? Gender { get; set; }
    }

}
