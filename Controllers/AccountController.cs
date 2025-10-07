using System.Text;
using System.Text.Json;
using CosmeticShopWeb.Models; 
using Microsoft.AspNetCore.Mvc;

namespace CosmeticShopWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("CosmeticApi");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new
            {
                LastName = model.LastName,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                Email = model.Email,
                Password = model.Password,
                Phone = model.Phone,
                RoleUs = model.RoleUs,
                StatusUs = model.StatusUs,
                Gender = model.Gender
            };

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("Users/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Регистрация успешна! Теперь войдите в систему.";
                return RedirectToAction("Login");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", error);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new { Username = model.Username, Password = model.Password };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync("Users/login", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Вы успешно вошли!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Ошибка входа: {error}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка подключения к API: {ex.Message}");
                return View(model);
            }
        }

    }
}
