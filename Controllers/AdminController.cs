using CosmeticShopWeb.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace CosmeticShopWeb.Controllers
{
    public class AdminController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5094/api/"; 
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        }

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "logs")
        {
            Console.WriteLine("=== GET Admin/Index вызван ===");

            if (!IsAdmin())
            {
                Console.WriteLine("Доступ запрещен: пользователь не админ");
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index", "Home");
            }

            var model = new AdminPanelViewModel { ActiveTab = tab };

            try
            {
                if (tab == "logs")
                {
                    model.AuditLogs = await GetAuditLogs();
                    Console.WriteLine($"Загружено логов: {model.AuditLogs.Count}");
                }
                else if (tab == "users")
                {
                    model.Users = await GetUsers();
                    Console.WriteLine($"Загружено пользователей: {model.Users.Count}");
                }
                else if (tab == "orders")
                {
                    model.Orders = await GetOrders();
                    Console.WriteLine($"Загружено заказов: {model.Orders.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в Admin/Index: {ex.Message}");
                TempData["Error"] = "Ошибка при загрузке данных";
            }

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteLog(int id)
        {
            Console.WriteLine($"=== POST Admin/DeleteLog вызван для ID: {id} ===");

            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}AuditLogs/{id}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Лог с ID {id} удален");
                    TempData["Success"] = "Лог успешно удален";
                }
                else
                {
                    Console.WriteLine($"Ошибка при удалении лога: {response.StatusCode}");
                    TempData["Error"] = "Ошибка при удалении лога";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в DeleteLog: {ex.Message}");
                TempData["Error"] = "Произошла ошибка при удалении";
            }

            return RedirectToAction("Index", new { tab = "logs" });
        }

        [HttpPost]
        public async Task<IActionResult> ClearAllLogs()
        {
            Console.WriteLine("=== POST Admin/ClearAllLogs вызван ===");

            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var logs = await GetAuditLogs();
                int deletedCount = 0;

                foreach (var log in logs)
                {
                    var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}AuditLogs/{log.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        deletedCount++;
                    }
                }

                Console.WriteLine($"Удалено логов: {deletedCount}");
                TempData["Success"] = $"Удалено {deletedCount} логов";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в ClearAllLogs: {ex.Message}");
                TempData["Error"] = "Произошла ошибка при очистке логов";
            }

            return RedirectToAction("Index", new { tab = "logs" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserStatus(int userId, string status)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var userResponse = await _httpClient.GetAsync($"{_apiBaseUrl}Users/{userId}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Пользователь не найден";
                    return RedirectToAction("Index", new { tab = "users" });
                }

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var currentUser = JsonConvert.DeserializeObject<ApiUserResponse>(userContent);

                var updateData = new
                {
                    LastName = currentUser.LastName,
                    FirstName = currentUser.FirstName,
                    MiddleName = currentUser.MiddleName,
                    Email = currentUser.Email,
                    Phone = currentUser.Phone,
                    RoleUs = currentUser.RoleUs,
                    StatusUs = status,
                    DateRegistered = currentUser.DateRegistered
                };

                var json = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}Users/{userId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Статус пользователя обновлен";
                }
                else
                {
                    TempData["Error"] = "Ошибка при обновлении статуса";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Произошла ошибка при обновлении статуса";
            }

            return RedirectToAction("Index", new { tab = "users" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            Console.WriteLine($"=== POST Admin/DeleteUser вызван для ID: {id} ===");

            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var userResponse = await _httpClient.GetAsync($"{_apiBaseUrl}Users/{id}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Пользователь не найден";
                    return RedirectToAction("Index", new { tab = "users" });
                }

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<ApiUserResponse>(userContent);

                if (user.RoleUs == "Администратор")
                {
                    TempData["Error"] = "Нельзя удалить пользователя с ролью Администратор";
                    return RedirectToAction("Index", new { tab = "users" });
                }

                var currentUser = GetUserFromCookie();
                if (currentUser?.Id_User == id)
                {
                    TempData["Error"] = "Нельзя удалить самого себя";
                    return RedirectToAction("Index", new { tab = "users" });
                }

                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}Users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Пользователь с ID {id} удален");
                    TempData["Success"] = "Пользователь успешно удален";
                }
                else
                {
                    Console.WriteLine($"Ошибка при удалении пользователя: {response.StatusCode}");
                    TempData["Error"] = "Ошибка при удалении пользователя";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в DeleteUser: {ex.Message}");
                TempData["Error"] = "Произошла ошибка при удалении пользователя";
            }

            return RedirectToAction("Index", new { tab = "users" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}Products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Товар успешно удален";
                }
                else
                {
                    TempData["Error"] = "Ошибка при удалении товара";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Произошла ошибка при удалении товара";
            }

            return RedirectToAction("Index", new { tab = "products" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var orderResponse = await _httpClient.GetAsync($"{_apiBaseUrl}Orders/{orderId}");
                if (!orderResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Заказ не найден";
                    return RedirectToAction("Index", new { tab = "orders" });
                }

                var orderContent = await orderResponse.Content.ReadAsStringAsync();
                var currentOrder = JsonConvert.DeserializeObject<ApiOrderResponse>(orderContent);

                var updateData = new
                {
                    UserId = currentOrder.UserId,
                    OrderDate = currentOrder.OrderDate,
                    TotalAmount = currentOrder.TotalAmount,
                    StatusOr = status,
                    DeliveryAddress = currentOrder.DeliveryAddress,
                    PromoId = currentOrder.PromoId
                };

                var json = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}Orders/{orderId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Статус заказа обновлен";
                }
                else
                {
                    TempData["Error"] = "Ошибка при обновлении статуса заказа";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Произошла ошибка при обновлении статуса заказа";
            }

            return RedirectToAction("Index", new { tab = "orders" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            Console.WriteLine($"=== POST Admin/DeleteOrder вызван для ID: {id} ===");

            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}Orders/{id}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Заказ с ID {id} удален");
                    TempData["Success"] = "Заказ успешно удален";
                }
                else
                {
                    Console.WriteLine($"Ошибка при удалении заказа: {response.StatusCode}");
                    TempData["Error"] = "Ошибка при удалении заказа";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в DeleteOrder: {ex.Message}");
                TempData["Error"] = "Произошла ошибка при удалении заказа";
            }

            return RedirectToAction("Index", new { tab = "orders" });
        }

        private async Task<List<AuditLogViewModel>> GetAuditLogs()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}AuditLogs");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ошибка API при получении логов: {response.StatusCode}");
                    return new List<AuditLogViewModel>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiLogs = JsonConvert.DeserializeObject<List<ApiAuditLogResponse>>(responseContent);

                return apiLogs?.Select(log => new AuditLogViewModel
                {
                    Id = log.Id_Log,
                    UserId = log.UserID,
                    UserName = log.UserName,
                    TableName = log.TableName,
                    ActionType = log.ActionType,
                    OldData = log.OldData,
                    NewData = log.NewData,
                    Timestamp = log.TimestampMl
                }).ToList() ?? new List<AuditLogViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetAuditLogs: {ex.Message}");
                return new List<AuditLogViewModel>();
            }
        }

        private async Task<List<UserViewModel>> GetUsers()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Users");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ошибка API при получении пользователей: {response.StatusCode}");
                    return new List<UserViewModel>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Users Response: {responseContent}");

                var apiUsers = JsonConvert.DeserializeObject<List<ApiUserResponse>>(responseContent);

                var users = apiUsers?.Select(user => new UserViewModel
                {
                    Id = user.IdUser,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleUs = user.RoleUs,
                    Status = user.StatusUs,
                    RegistrationDate = user.DateRegistered
                }).ToList() ?? new List<UserViewModel>();

                Console.WriteLine($"Преобразовано пользователей: {users.Count}");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetUsers: {ex.Message}");
                return new List<UserViewModel>();
            }
        }


        private async Task<List<AdminOrderViewModel>> GetOrders()
        {
            try
            {
                Console.WriteLine($"=== Получение заказов из API: {_apiBaseUrl}Orders ===");

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Orders");

                Console.WriteLine($"Статус ответа: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ошибка API при получении заказов: {response.StatusCode}");
                    return new List<AdminOrderViewModel>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ответ API: {responseContent}");

                var apiOrders = JsonConvert.DeserializeObject<List<ApiOrderResponse>>(responseContent);
                Console.WriteLine($"Десериализовано заказов: {apiOrders?.Count ?? 0}");

                var orders = new List<AdminOrderViewModel>();
                foreach (var apiOrder in apiOrders)
                {
                    var user = await GetUserById(apiOrder.UserId);
                    Console.WriteLine($"Заказ {apiOrder.IdOrder}, пользователь: {user?.FirstName} {user?.LastName}");

                    orders.Add(new AdminOrderViewModel
                    {
                        Id = apiOrder.IdOrder,
                        UserName = user?.FirstName + " " + user?.LastName,
                        UserEmail = user?.Email,
                        OrderDate = apiOrder.OrderDate,
                        TotalAmount = apiOrder.TotalAmount,
                        Status = apiOrder.StatusOr,
                        DeliveryAddress = apiOrder.DeliveryAddress
                    });
                }

                Console.WriteLine($"Итоговое количество заказов: {orders.Count}");
                return orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в GetOrders: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return new List<AdminOrderViewModel>();
            }
        }

        private async Task<ApiUserResponse> GetUserById(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ApiUserResponse>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пользователя: {ex.Message}");
            }
            return null;
        }
        [HttpGet("register-employee")]
        public IActionResult RegisterEmployee()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            var model = new RegisterEmployeeViewModel();
            return View(model);
        }

        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee(RegisterEmployeeViewModel model)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registerData = new
                {
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    Email = model.Email,
                    Phone = model.Phone,
                    RoleUs = model.RoleUs,
                    Gender = model.Gender
                };

                var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}Users/register-employee", registerData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    TempData["Success"] = result?.Message?.ToString() ?? "Сотрудник успешно зарегистрирован";
                    return RedirectToAction("Index", new { tab = "users" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = errorContent;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Произошла ошибка: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet("backup")]
        public async Task<IActionResult> Backup()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index", "Home");
            }

            var model = new BackupViewModel();

            try
            {
                var response = await _httpClient.GetAsync("Users/backup-files");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var backupFiles = System.Text.Json.JsonSerializer.Deserialize<List<string>>(content, _jsonOptions);
                    model.BackupFiles = backupFiles ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                model.Message = $"Ошибка при загрузке списка бэкапов: {ex.Message}";
            }

            return View(model);
        }

        [HttpPost("create-backup")]
        public async Task<IActionResult> CreateBackup()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Backup");
            }

            try
            {
                var response = await _httpClient.PostAsync("Users/backup", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    TempData["Success"] = "Резервная копия успешно создана!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Ошибка при создании бэкапа: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("Backup");
        }


        [HttpPost("delete-backup")]
        public async Task<IActionResult> DeleteBackup(string fileName)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Backup");
            }

            try
            {
                var backupPath = Path.Combine(Directory.GetCurrentDirectory(), "Backups", fileName);

                if (System.IO.File.Exists(backupPath))
                {
                    System.IO.File.Delete(backupPath);
                    TempData["Success"] = "Файл бэкапа успешно удален!";
                }
                else
                {
                    TempData["Error"] = "Файл не найден";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToAction("Backup");
        }

        [HttpPost("restore-backup")]
        public async Task<IActionResult> RestoreBackup(IFormFile file)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Backup");
            }

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Файл не выбран";
                return RedirectToAction("Backup");
            }

            try
            {
                using var formData = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream());
                formData.Add(fileContent, "file", file.FileName);

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}Users/restore", formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "База данных успешно восстановлена!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Ошибка при восстановлении: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("Backup");
        }
        [HttpGet("create-backup-page")]
        public IActionResult CreateBackupPage()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost("create-backup-now")]
        public async Task<IActionResult> CreateBackupNow()
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Доступ запрещен" });
            }

            try
            {
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}Users/backup", null);

                if (response.IsSuccessStatusCode)
                {
                    var resultContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var result = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(resultContent);
                        var message = result.GetProperty("message").GetString();
                        return Json(new { success = true, message = message });
                    }
                    catch
                    {
                        return Json(new { success = true, message = resultContent });
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Ошибка API: {error}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ошибка соединения: {ex.Message}" });
            }
        }
    }
}