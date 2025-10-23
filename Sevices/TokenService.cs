using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CosmeticShopAPI.Services
{
    public class TokenService
    {
        private readonly string _secretKey;

        public TokenService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Secret"];
        }

        public string GeneratePasswordResetToken(string email, int userId)
        {
            Console.WriteLine($"[TOKEN_GENERATE] Generating token for Email: {email}, UserId: {userId}");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("email", email),
            new Claim("userId", userId.ToString()),
            new Claim("purpose", "password_reset")
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            Console.WriteLine($"[TOKEN_GENERATE] Token generated: {tokenString}");
            return tokenString;
        }

        public (bool isValid, string email, int userId) ValidatePasswordResetToken(string token)
        {
            Console.WriteLine($"[TOKEN_VALIDATE] Validating token: {token}");

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                Console.WriteLine($"[TOKEN_VALIDATE] All claims in token:");
                foreach (var claim in principal.Claims)
                {
                    Console.WriteLine($"[TOKEN_VALIDATE]   - {claim.Type}: {claim.Value}");
                }

                var purpose = principal.FindFirst("purpose")?.Value;
                if (purpose != "password_reset")
                {
                    Console.WriteLine($"[TOKEN_VALIDATE] Invalid purpose: {purpose}");
                    return (false, null, 0);
                }

                var email = principal.FindFirst("email")?.Value ??
                           principal.FindFirst(ClaimTypes.Email)?.Value ??
                           principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

                var userId = int.Parse(principal.FindFirst("userId")?.Value);

                Console.WriteLine($"[TOKEN_VALIDATE] Token VALID - Email: {email}, UserId: {userId}");
                return (true, email, userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TOKEN_VALIDATE] Token validation FAILED: {ex.Message}");
                return (false, null, 0);
            }
        }
    }
}