using CosmeticShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace CosmeticShopWeb.Controllers
{
    public class OrderController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public OrderController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5094/api/";
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            Console.WriteLine("=== GET Checkout вызван ===");

            var cart = GetCart();
            Console.WriteLine($"Товаров в корзине: {cart.Items.Count}");

            if (!cart.Items.Any())
            {
                Console.WriteLine("Корзина пуста, редирект в корзину");
                TempData["Error"] = "Корзина пуста. Добавьте товары перед оформлением заказа.";
                return RedirectToAction("Index", "Cart");
            }

            var currentUser = GetUserFromCookie();
            Console.WriteLine($"Пользователь из куки: {(currentUser != null ? $"ID={currentUser.Id_User}, Email={currentUser.Email}" : "не авторизован")}");

            var model = new CheckoutViewModel
            {
                Cart = cart,
                DeliveryCost = CalculateDeliveryCost("standard"),
                DeliveryMethod = "standard", 
                PaymentMethod = "card"       
            };

            if (currentUser != null)
            {
                var fullUserData = await GetFullUserData(currentUser.Id_User);

                if (fullUserData != null)
                {
                    model.FirstName = fullUserData.FirstName ?? currentUser.FirstName;
                    model.LastName = fullUserData.LastName ?? currentUser.LastName;
                    model.Email = fullUserData.Email ?? currentUser.Email;
                    model.Phone = fullUserData.Phone;

                    Console.WriteLine($"Данные из API: {model.FirstName} {model.LastName}, {model.Email}, {model.Phone}");
                }
                else
                {
                    model.FirstName = currentUser.FirstName;
                    model.LastName = currentUser.LastName;
                    model.Email = currentUser.Email;
                    Console.WriteLine($"Данные из куки: {model.FirstName} {model.LastName}, {model.Email}");
                }
            }

            ViewBag.DeliveryOptions = GetDeliveryOptions();
            ViewBag.PaymentOptions = GetPaymentOptions();

            Console.WriteLine("Страница оформления заказа показана");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            Console.WriteLine("=== POST Checkout вызван ===");

            var cart = GetCart();
            Console.WriteLine($"Товаров в корзине: {cart.Items.Count}");

            if (!cart.Items.Any())
            {
                Console.WriteLine("Корзина пуста в POST запросе");
                TempData["Error"] = "Корзина пуста. Добавьте товары перед оформлением заказа.";
                return RedirectToAction("Index", "Cart");
            }

            var currentUser = GetUserFromCookie();
            Console.WriteLine($"Пользователь из куки: {(currentUser != null ? $"ID={currentUser.Id_User}, Email={currentUser.Email}" : "не авторизован")}");

            if (currentUser != null)
            {
                model.FirstName = currentUser.FirstName;
                model.LastName = currentUser.LastName;
                model.Email = currentUser.Email;

                var fullUserData = await GetFullUserData(currentUser.Id_User);
                model.Phone = fullUserData?.Phone ?? model.Phone;

                Console.WriteLine($"Данные из куки установлены: {model.FirstName} {model.LastName}, {model.Email}");
            }

            model.Cart = cart;
            model.DeliveryCost = CalculateDeliveryCost(model.DeliveryMethod);

            ModelState.Remove("Cart");
            if (currentUser != null)
            {
                ModelState.Remove("FirstName");
                ModelState.Remove("LastName");
                ModelState.Remove("Email");
                ModelState.Remove("Phone");
            }

            Console.WriteLine($"Модель валидна: {ModelState.IsValid}");
            Console.WriteLine($"Способ оплаты: {model.PaymentMethod}");
            Console.WriteLine($"Способ доставки: {model.DeliveryMethod}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Модель невалидна, возвращаю с ошибками");

                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"Ошибка в поле '{key}':");
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"  - {error.ErrorMessage}");
                        }
                    }
                }

                ViewBag.DeliveryOptions = GetDeliveryOptions();
                ViewBag.PaymentOptions = GetPaymentOptions();
                return View(model);
            }

            try
            {
                Console.WriteLine("Начинаю оформление заказа...");

                var userId = currentUser?.Id_User ?? 0;
                Console.WriteLine($"UserID для заказа: {userId}");

                var orderData = new
                {
                    UserID = userId,
                    OrderDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalAmount = model.TotalAmount,
                    DeliveryAddress = $"{model.City}, {model.Address}, {model.PostalCode}",
                    CustomerName = $"{model.FirstName} {model.LastName}",
                    CustomerEmail = model.Email,
                    CustomerPhone = model.Phone,
                    DeliveryMethod = model.DeliveryMethod,
                    PaymentMethod = model.PaymentMethod,
                    Comment = model.Comment,
                    CartItems = cart.Items
                };

                var orderJson = JsonConvert.SerializeObject(orderData);
                HttpContext.Session.SetString("PendingOrder", orderJson);
                Console.WriteLine("Данные заказа сохранены в сессии");

                if (model.PaymentMethod == "cash")
                {
                    Console.WriteLine("Обработка наличной оплаты...");
                    var orderId = await CreateOrderInDatabase(orderData, "В обработке");
                    Console.WriteLine($"Заказ создан с ID: {orderId}");

                    ClearCart();
                    HttpContext.Session.Remove("PendingOrder");
                    Console.WriteLine("Корзина очищена");

                    Console.WriteLine($"Редирект на Confirmation с ID: {orderId}");
                    return RedirectToAction("Confirmation", new { id = orderId });
                }
                else
                {
                    Console.WriteLine($"Редирект на Payment с методом: {model.PaymentMethod}");
                    return RedirectToAction("Payment", new { paymentMethod = model.PaymentMethod });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в Checkout: {ex.Message}");
                TempData["Error"] = "Ошибка при оформлении заказа. Попробуйте позже.";
                ViewBag.DeliveryOptions = GetDeliveryOptions();
                ViewBag.PaymentOptions = GetPaymentOptions();
                return View(model);
            }
        }

        private async Task<ApiUserResponse> GetFullUserData(int userId)
        {
            try
            {
                Console.WriteLine($"Получаю данные пользователя {userId} из API...");
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ответ API: {responseContent}");

                    var userData = System.Text.Json.JsonSerializer.Deserialize<ApiUserResponse>(responseContent, _jsonOptions);
                    Console.WriteLine($"Получены данные: {userData?.FirstName} {userData?.LastName}, Phone: {userData?.Phone}");
                    return userData;
                }
                else
                {
                    Console.WriteLine($"Ошибка API: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных пользователя: {ex.Message}");
                return null;
            }
        }

        [HttpPost]
        public async Task<JsonResult> ProcessCashPayment()
        {
            Console.WriteLine("=== ProcessCashPayment (AJAX) вызван ===");

            try
            {
                var pendingOrderJson = HttpContext.Session.GetString("PendingOrder");
                Console.WriteLine($"Pending order в сессии: {!string.IsNullOrEmpty(pendingOrderJson)}");

                if (string.IsNullOrEmpty(pendingOrderJson))
                {
                    Console.WriteLine("ОШИБКА: Нет данных заказа в сессии");
                    return Json(new { success = false, error = "Данные заказа не найдены" });
                }

                var orderData = JsonConvert.DeserializeObject<dynamic>(pendingOrderJson);
                Console.WriteLine("Создаю заказ через AJAX...");

                var orderId = await CreateOrderInDatabase(orderData, "В обработке");
                Console.WriteLine($"Заказ создан через AJAX с ID: {orderId}");

                HttpContext.Session.Remove("PendingOrder");
                ClearCart();
                Console.WriteLine("Сессия и корзина очищены");

                return Json(new { success = true, orderId = orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в ProcessCashPayment: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Payment(string paymentMethod)
        {
            Console.WriteLine($"=== GET Payment вызван с методом: {paymentMethod} ===");

            var pendingOrderJson = HttpContext.Session.GetString("PendingOrder");
            Console.WriteLine($"Pending order в сессии: {!string.IsNullOrEmpty(pendingOrderJson)}");

            if (string.IsNullOrEmpty(pendingOrderJson))
            {
                Console.WriteLine("ОШИБКА: Нет данных заказа для страницы оплаты");
                TempData["Error"] = "Данные заказа не найдены";
                return RedirectToAction("Checkout");
            }

            var orderData = JsonConvert.DeserializeObject<dynamic>(pendingOrderJson);

            var model = new PaymentViewModel
            {
                PaymentMethod = paymentMethod,
                TotalAmount = (decimal)orderData.TotalAmount
            };

            Console.WriteLine("Страница оплаты показана");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            Console.WriteLine("=== POST ProcessPayment вызван ===");
            Console.WriteLine($"Метод оплаты: {model.PaymentMethod}");

            ModelState.Remove("CardHolderName");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Модель невалидна, возвращаю с ошибками");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"Ошибка в поле '{key}':");
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"  - {error.ErrorMessage}");
                        }
                    }
                }
                return View("Payment", model);
            }

            try
            {
                var pendingOrderJson = HttpContext.Session.GetString("PendingOrder");
                if (string.IsNullOrEmpty(pendingOrderJson))
                {
                    Console.WriteLine("ОШИБКА: Нет данных заказа для обработки платежа");
                    TempData["Error"] = "Данные заказа не найдены";
                    return RedirectToAction("Checkout");
                }

                var orderData = JsonConvert.DeserializeObject<dynamic>(pendingOrderJson);
                Console.WriteLine("Создаю заказ после оплаты...");

                var orderId = await CreateOrderInDatabase(orderData, "В обработке");
                Console.WriteLine($"Заказ создан с ID: {orderId}");

                HttpContext.Session.Remove("PendingOrder");
                ClearCart();
                Console.WriteLine("Сессия и корзина очищены");

                Console.WriteLine($"Редирект на Confirmation с ID: {orderId}");
                return RedirectToAction("Confirmation", new { id = orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в ProcessPayment: {ex.Message}");
                TempData["Error"] = "Ошибка при обработке платежа";
                return View("Payment", model);
            }
        }

        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            Console.WriteLine($"=== GET Confirmation вызван с ID: {id} ===");
            ViewBag.OrderId = id;
            Console.WriteLine("Страница подтверждения заказа показана");
            return View();
        }

        private async Task<int> CreateOrderInDatabase(dynamic orderData, string status)
        {
            Console.WriteLine("=== CreateOrderInDatabase начал работу ===");
            Console.WriteLine($"Статус заказа: {status}");

            try
            {
                var orderRequest = new
                {
                    UserID = (int)orderData.UserID,
                    OrderDate = (string)orderData.OrderDate,
                    TotalAmount = (decimal)orderData.TotalAmount,
                    StatusOr = status,
                    DeliveryAddress = (string)orderData.DeliveryAddress,
                    PromoID = (int?)null
                };

                Console.WriteLine($"Данные заказа: UserID={orderData.UserID}, TotalAmount={orderData.TotalAmount}");

                var orderJson = JsonConvert.SerializeObject(orderRequest);
                var orderContent = new StringContent(orderJson, Encoding.UTF8, "application/json");

                var apiUrl = $"{_apiBaseUrl}orders";
                Console.WriteLine($"Отправляю запрос на API: {apiUrl}");

                var orderResponse = await _httpClient.PostAsync(apiUrl, orderContent);
                Console.WriteLine($"Ответ API: {orderResponse.StatusCode}");

                if (!orderResponse.IsSuccessStatusCode)
                {
                    var errorContent = await orderResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"ОШИБКА API: {orderResponse.StatusCode}, Content: {errorContent}");
                    throw new Exception($"Ошибка API: {orderResponse.StatusCode}");
                }

                var orderResponseContent = await orderResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Ответ от API: {orderResponseContent}");

                var createdOrder = JsonConvert.DeserializeObject<OrderCreateResponse>(orderResponseContent);
                Console.WriteLine($"Заказ создан с ID: {createdOrder.Id_Order}");

                var cartItems = orderData.CartItems;
                Console.WriteLine($"Создаю {cartItems.Count} деталей заказа");

                foreach (var item in cartItems)
                {
                    var productId = (int)item.ProductId;
                    var quantity = (int)item.Quantity;
                    var price = (decimal)item.Price;

                    Console.WriteLine($"Добавляю товар {productId}, количество {quantity}");

                    var orderDetailData = new
                    {
                        OrderID = createdOrder.Id_Order,
                        ProductID = productId,
                        Quantity = quantity,
                        Price = price
                    };

                    var detailJson = JsonConvert.SerializeObject(orderDetailData);
                    var detailContent = new StringContent(detailJson, Encoding.UTF8, "application/json");

                    var detailResponse = await _httpClient.PostAsync($"{_apiBaseUrl}orderdetails", detailContent);
                    Console.WriteLine($"Деталь заказа создана, статус: {detailResponse.StatusCode}");
                }

                Console.WriteLine("Все детали заказа созданы успешно");
                return createdOrder.Id_Order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в CreateOrderInDatabase: {ex.Message}");
                throw new Exception($"Ошибка создания заказа: {ex.Message}");
            }
        }

        private List<DeliveryOption> GetDeliveryOptions()
        {
            return new List<DeliveryOption>
            {
                new DeliveryOption { Id = "standard", Name = "Стандартная доставка", Description = "3-5 рабочих дней", Cost = 300 },
                new DeliveryOption { Id = "express", Name = "Экспресс доставка", Description = "1-2 рабочих дня", Cost = 600 },
                new DeliveryOption { Id = "pickup", Name = "Самовывоз", Description = "Бесплатно из пункта выдачи", Cost = 0 }
            };
        }

        private List<PaymentOption> GetPaymentOptions()
        {
            return new List<PaymentOption>
            {
                new PaymentOption { Id = "card", Name = "Банковская карта", Description = "Оплата онлайн картой", Icon = "💳" },
                new PaymentOption { Id = "cash", Name = "Наличные", Description = "Оплата при получении", Icon = "💵" },
                new PaymentOption { Id = "sbp", Name = "СБП", Description = "Система быстрых платежей", Icon = "📱" }
            };
        }

        private decimal CalculateDeliveryCost(string deliveryMethod)
        {
            return deliveryMethod switch
            {
                "express" => 600,
                "standard" => 300,
                "pickup" => 0,
                _ => 300
            };
        }

        private CartViewModel GetCart()
        {
            var json = HttpContext.Session.GetString("CartSession");
            return json == null ? new CartViewModel() : JsonConvert.DeserializeObject<CartViewModel>(json);
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove("CartSession");
        }
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            Console.WriteLine("=== GET MyOrders вызван ===");

            var currentUser = GetUserFromCookie();
            if (currentUser == null)
            {
                Console.WriteLine("Пользователь не авторизован, редирект на логин");
                TempData["Error"] = "Для просмотра заказов необходимо авторизоваться";
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine($"Получаю заказы для пользователя ID: {currentUser.Id_User}");

            try
            {
                var orders = await GetUserOrders(currentUser.Id_User);
                Console.WriteLine($"Получено заказов: {orders.Count}");

                return View(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА при получении заказов: {ex.Message}");
                TempData["Error"] = "Ошибка при загрузке заказов";
                return View(new List<OrderHistoryViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            Console.WriteLine($"=== GET OrderDetails вызван для заказа ID: {id} ===");

            var currentUser = GetUserFromCookie();
            if (currentUser == null)
            {
                TempData["Error"] = "Для просмотра заказа необходимо авторизоваться";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await GetOrderDetails(id, currentUser.Id_User);
                if (order == null)
                {
                    TempData["Error"] = "Заказ не найден";
                    return RedirectToAction("MyOrders");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА при получении деталей заказа: {ex.Message}");
                TempData["Error"] = "Ошибка при загрузке заказа";
                return RedirectToAction("MyOrders");
            }
        }

        private async Task<List<OrderHistoryViewModel>> GetUserOrders(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}orders");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ошибка API при получении заказов: {response.StatusCode}");
                    return new List<OrderHistoryViewModel>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ответ API orders: {responseContent}");

                var allOrders = JsonConvert.DeserializeObject<List<ApiOrderResponse>>(responseContent);

                var userOrders = allOrders?.Where(o => o.UserId == userId).ToList() ?? new List<ApiOrderResponse>();

                var orders = new List<OrderHistoryViewModel>();
                foreach (var apiOrder in userOrders)
                {
                    var orderDetails = await GetOrderItems(apiOrder.IdOrder);

                    orders.Add(new OrderHistoryViewModel
                    {
                        Id = apiOrder.IdOrder,
                        OrderDate = apiOrder.OrderDate,
                        TotalAmount = apiOrder.TotalAmount,
                        Status = apiOrder.StatusOr,
                        DeliveryAddress = apiOrder.DeliveryAddress,
                        PaymentMethod = "card", 
                        Items = orderDetails
                    });
                }

                return orders.OrderByDescending(o => o.OrderDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetUserOrders: {ex.Message}");
                return new List<OrderHistoryViewModel>();
            }
        }

        private async Task<OrderHistoryViewModel> GetOrderDetails(int orderId, int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}orders/{orderId}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ошибка API при получении заказа: {response.StatusCode}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiOrder = JsonConvert.DeserializeObject<ApiOrderResponse>(responseContent);

                if (apiOrder?.UserId != userId)
                {
                    Console.WriteLine($"Заказ {orderId} не принадлежит пользователю {userId}");
                    return null;
                }

                var orderDetails = await GetOrderItems(orderId);

                return new OrderHistoryViewModel
                {
                    Id = apiOrder.IdOrder,
                    OrderDate = apiOrder.OrderDate,
                    TotalAmount = apiOrder.TotalAmount,
                    Status = apiOrder.StatusOr,
                    DeliveryAddress = apiOrder.DeliveryAddress,
                    PaymentMethod = "card",
                    Items = orderDetails
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetOrderDetails: {ex.Message}");
                return null;
            }
        }

        private async Task<List<OrderItemViewModel>> GetOrderItems(int orderId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}orderdetails");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<OrderItemViewModel>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var allDetails = JsonConvert.DeserializeObject<List<ApiOrderDetailResponse>>(responseContent);

                var orderDetails = allDetails?.Where(d => d.OrderID == orderId).ToList() ?? new List<ApiOrderDetailResponse>();

                var items = new List<OrderItemViewModel>();
                foreach (var detail in orderDetails)
                {
                    var product = await GetProductInfo(detail.ProductID);

                    items.Add(new OrderItemViewModel
                    {
                        ProductName = product?.Name ?? "Товар",
                        Quantity = detail.Quantity,
                        Price = detail.Price,
                        ImageUrl = product?.ImageUrl ?? "/images/placeholder-product.jpg"
                    });
                }

                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetOrderItems: {ex.Message}");
                return new List<OrderItemViewModel>();
            }
        }

        private async Task<ProductInfo> GetProductInfo(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}products/{productId}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProductInfo>(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении информации о товаре: {ex.Message}");
                return null;
            }
        }
    }

}