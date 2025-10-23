using CosmeticShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace CosmeticShopWeb.Controllers
{
    public class AccountController : BaseController
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
           : base(configuration)
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

            var json = System.Text.Json.JsonSerializer.Serialize(dto);
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
            if (IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new { Username = model.Username, Password = model.Password };
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync("Users/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var userData = System.Text.Json.JsonSerializer.Deserialize<UserLoginResponse>(responseContent, _jsonOptions);

                    if (userData != null)
                    {
                        SaveUserToCookie(userData, model.RememberMe);
                        TempData["Message"] = "Вы успешно вошли!";
                        return RedirectToAction("Index", "Home");
                    }
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

            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("UserAuth");
            TempData["Message"] = "Вы успешно вышли из системы!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentUser = CurrentUser;
            if (currentUser == null)
                return RedirectToAction("Login");

            var fullUserData = await GetFullUserData(currentUser.Id_User);

            var model = new ProfileViewModel
            {
                FirstName = fullUserData.FirstName ?? currentUser.FirstName,
                LastName = fullUserData.LastName ?? currentUser.LastName,
                Email = fullUserData.Email ?? currentUser.Email,
                RoleUs = fullUserData.RoleUs ?? currentUser.RoleUs,
                DateRegistered = fullUserData.DateRegistered,
                StatusUs = fullUserData.StatusUs
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var currentUser = CurrentUser;
            if (currentUser == null)
                return RedirectToAction("Login");

            var model = new UpdateProfileViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Email = currentUser.Email
            };

            try
            {
                var response = await _http.GetAsync($"Users/{currentUser.Id_User}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiUserData = System.Text.Json.JsonSerializer.Deserialize<ApiUserResponse>(responseContent, _jsonOptions);

                    if (apiUserData != null)
                    {
                        model.FirstName = apiUserData.FirstName ?? model.FirstName;
                        model.LastName = apiUserData.LastName ?? model.LastName;
                        model.Email = apiUserData.Email ?? model.Email;
                        model.MiddleName = apiUserData.MiddleName;
                        model.Phone = apiUserData.Phone;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных для редактирования: {ex.Message}");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUser = CurrentUser;
            if (currentUser == null)
                return RedirectToAction("Login");

            try
            {
                var currentUserData = await GetFullUserData(currentUser.Id_User);

                var updateData = new
                {
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = string.IsNullOrEmpty(model.NewPassword) ? null : model.NewPassword,
                    RoleUs = currentUserData.RoleUs,
                    StatusUs = currentUserData.StatusUs,
                    DateRegistered = currentUserData.DateRegistered
                };

                var json = System.Text.Json.JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PutAsync($"Users/{currentUser.Id_User}", content);

                if (response.IsSuccessStatusCode)
                {
                    var updatedUser = new UserLoginResponse
                    {
                        Id_User = currentUser.Id_User,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        RoleUs = currentUserData.RoleUs
                    };

                    SaveUserToCookie(updatedUser, true);

                    TempData["Message"] = "Профиль успешно обновлен!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Ошибка обновления: {error}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка подключения: {ex.Message}");
                return View(model);
            }
        }

        private async Task<ApiUserResponse> GetFullUserData(int userId)
        {
            try
            {
                var response = await _http.GetAsync($"Users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<ApiUserResponse>(responseContent, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных пользователя: {ex.Message}");
            }

            return new ApiUserResponse
            {
                RoleUs = "Клиент",
                StatusUs = "Активен",
                DateRegistered = DateTime.Now.Date
            };
        }

        private void SaveUserToCookie(UserLoginResponse user, bool rememberMe)
        {
            var userData = new
            {
                user.Id_User,
                user.FirstName,
                user.LastName,
                user.RoleUs,
                user.Email
            };

            var userJson = JsonConvert.SerializeObject(userData);
            var encryptedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(userJson));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            if (rememberMe)
            {
                cookieOptions.Expires = DateTimeOffset.Now.AddDays(30);
            }

            Response.Cookies.Append("UserAuth", encryptedData, cookieOptions);
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _http.PostAsJsonAsync("Users/forgot-password", new { Email = model.Email });


                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Если email существует, инструкции по восстановлению пароля будут отправлены на ваш email.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Неверная ссылка для сброса пароля";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var resetData = new
                {
                    Token = model.Token,
                    Password = model.Password
                };

                var response = await _http.PostAsJsonAsync("Users/reset-password", resetData);


                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Пароль успешно изменен! Теперь вы можете войти с новым паролем.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка: {ex.Message}");
                return View(model);
            }
        }
    }
}