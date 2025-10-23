using CosmeticShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CosmeticShopWeb.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected UserLoginResponse CurrentUser
        {
            get
            {
                return GetUserFromCookie();
            }
        }
        protected bool IsAdmin()
        {
            var user = GetUserFromCookie();
            return user?.RoleUs == "Администратор"; 
        }
        protected bool IsAuthenticated => CurrentUser != null;

        protected UserLoginResponse GetUserFromCookie()
        {
            if (Request.Cookies.TryGetValue("UserAuth", out var encryptedData))
            {
                try
                {
                    var userJson = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData));
                    return JsonSerializer.Deserialize<UserLoginResponse>(userJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    Response.Cookies.Delete("UserAuth");
                    return null;
                }
            }
            return null;
        }

        protected async Task<bool> UserHasReviewForProduct(int userId, int productId)
        {
            try
            {
                var httpClient = new HttpClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5094/api/";

                var url = $"{apiBaseUrl}reviews/user/{userId}/product/{productId}";
                Console.WriteLine($"🔍🔍🔍 ПРОВЕРКА ОТЗЫВА ДО ДОБАВЛЕНИЯ:");
                Console.WriteLine($"🔍 URL: {url}");
                Console.WriteLine($"🔍 User: {userId}, Product: {productId}");

                var response = await httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🔍 Статус проверки: {response.StatusCode}");
                Console.WriteLine($"🔍 Ответ проверки: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ReviewViewModel>>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var hasReview = apiResponse?.Success == true && apiResponse.Data != null;
                    Console.WriteLine($"🔍🔍🔍 РЕЗУЛЬТАТ ПРОВЕРКИ: {hasReview}");

                    if (hasReview)
                    {
                        Console.WriteLine($"🔍 Найден отзыв: {apiResponse.Data.IdReview} от {apiResponse.Data.UserName}");
                    }
                    else
                    {
                        Console.WriteLine($"🔍 Отзыв не найден (data is null)");
                    }

                    return hasReview;
                }
                else
                {
                    Console.WriteLine($"🔍 Ошибка HTTP при проверке: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 EXCEPTION при проверке отзыва: {ex.Message}");
                return false;
            }
        }

        public class UserLoginResponse
        {
            public int Id_User { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string RoleUs { get; set; }
            public string Email { get; set; }
        }
    }
}