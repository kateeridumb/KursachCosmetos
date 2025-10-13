using CosmeticShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CosmeticShopWeb.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
           : base(configuration) 
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5094/api/";
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private const int PageSize = 12;

        public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1)
        {
            try
            {
                var url = $"{_apiBaseUrl}products?page={page}&pageSize={PageSize}";

                if (categoryId.HasValue && categoryId > 0)
                    url += $"&categoryId={categoryId}";

                if (!string.IsNullOrEmpty(search))
                    url += $"&search={Uri.EscapeDataString(search)}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return View(await BuildEmptyListModel(categoryId, search));
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductViewModel>>>(json, _jsonOptions);

                if (apiResponse?.Success != true || apiResponse.Data == null)
                {
                    return View(await BuildEmptyListModel(categoryId, search));
                }

                var categories = await GetCategoriesAsync();

                var viewModel = new ProductListViewModel
                {
                    Products = apiResponse.Data,
                    Categories = categories,
                    SelectedCategoryId = categoryId ?? 0,
                    SearchTerm = search ?? "",
                    CurrentPage = page,
                    TotalPages = apiResponse.TotalPages > 0 ? apiResponse.TotalPages : 1,
                    TotalCount = apiResponse.TotalCount
                };

                return View(viewModel);
            }
            catch
            {
                return View(await BuildEmptyListModel(categoryId, search));
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var url = $"{_apiBaseUrl}products/{id}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductViewModel>>(json, _jsonOptions);

                if (apiResponse?.Success != true || apiResponse.Data == null)
                    return NotFound();

                var product = apiResponse.Data;

                // Загружаем отзывы для продукта
                await LoadProductReviews(product);

                try
                {
                    var imagesUrl = $"{_apiBaseUrl}images/product/{id}";
                    var imagesResponse = await _httpClient.GetAsync(imagesUrl);

                    if (imagesResponse.IsSuccessStatusCode)
                    {
                        var imagesJson = await imagesResponse.Content.ReadAsStringAsync();
                        var imagesApi = JsonSerializer.Deserialize<ApiResponse<List<string>>>(imagesJson, _jsonOptions);

                        if (imagesApi?.Success == true && imagesApi.Data != null)
                        {
                            product.ImageUrls = imagesApi.Data;
                            product.MainImageUrl = product.ImageUrls.FirstOrDefault() ?? "";
                        }
                    }
                }
                catch
                {
                    product.ImageUrls = new List<string>();
                    product.MainImageUrl = "";
                }

                try
                {
                    var relatedUrl = $"{_apiBaseUrl}products?categoryId={product.CategoryId}&pageSize=4";
                    var relatedResponse = await _httpClient.GetAsync(relatedUrl);

                    if (relatedResponse.IsSuccessStatusCode)
                    {
                        var relatedJson = await relatedResponse.Content.ReadAsStringAsync();
                        var relatedApi = JsonSerializer.Deserialize<ApiResponse<List<ProductViewModel>>>(relatedJson, _jsonOptions);

                        if (relatedApi?.Success == true && relatedApi.Data != null)
                        {
                            product.RelatedProducts = relatedApi.Data
                                .Where(p => p.IdProduct != product.IdProduct)
                                .ToList();
                        }
                    }
                }
                catch
                {
                    product.RelatedProducts = new List<ProductViewModel>();
                }

                return View(product);
            }
            catch
            {
                return NotFound();
            }
        }
        private async Task LoadProductReviews(ProductViewModel product)
        {
            try
            {
                var reviewsUrl = $"{_apiBaseUrl}reviews/product/{product.IdProduct}";
                Console.WriteLine($"🔍 Загрузка отзывов по URL: {reviewsUrl}");

                var reviewsResponse = await _httpClient.GetAsync(reviewsUrl);
                var responseContent = await reviewsResponse.Content.ReadAsStringAsync();

                Console.WriteLine($"🔍 Статус загрузки отзывов: {reviewsResponse.StatusCode}");
                Console.WriteLine($"🔍 Ответ загрузки отзывов: {responseContent}");

                if (reviewsResponse.IsSuccessStatusCode)
                {
                    var reviewsApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ReviewViewModel>>>(responseContent, _jsonOptions);

                    if (reviewsApiResponse?.Success == true && reviewsApiResponse.Data != null)
                    {
                        product.Reviews = reviewsApiResponse.Data;
                        product.TotalReviews = product.Reviews.Count;
                        product.AverageRating = product.Reviews.Any() ?
                            Math.Round(product.Reviews.Average(r => r.Rating), 1) : 0;

                        Console.WriteLine($"✅ Загружено {product.Reviews.Count} отзывов, средний рейтинг: {product.AverageRating}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Ошибка в ответе API отзывов: {reviewsApiResponse?.Message}");
                        SetEmptyReviews(product);
                    }
                }
                else
                {
                    Console.WriteLine($"❌ HTTP ошибка при загрузке отзывов: {reviewsResponse.StatusCode}");
                    SetEmptyReviews(product);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPTION при загрузке отзывов: {ex.Message}");
                SetEmptyReviews(product);
            }
        }
        private void SetEmptyReviews(ProductViewModel product)
        {
            product.Reviews = new List<ReviewViewModel>();
            product.TotalReviews = 0;
            product.AverageRating = 0;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Пожалуйста, исправьте ошибки в форме";
                return RedirectToAction("Details", new { id = model.ProductId });
            }

            var currentUser = GetUserFromCookie();
            if (currentUser == null)
            {
                TempData["Error"] = "Для добавления отзыва необходимо войти в систему";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                Console.WriteLine($"=== ДОБАВЛЕНИЕ ОТЗЫВА ===");
                Console.WriteLine($"👤 User: {currentUser.Id_User}, 🎯 Product: {model.ProductId}");

                // ПРОВЕРКА СУЩЕСТВУЮЩЕГО ОТЗЫВА
                var hasExistingReview = await UserHasReviewForProduct(currentUser.Id_User, model.ProductId);
                Console.WriteLine($"🔍 Проверка отзыва: {hasExistingReview}");

                if (hasExistingReview)
                {
                    TempData["Error"] = "Вы уже оставляли отзыв на этот товар";
                    return RedirectToAction("Details", new { id = model.ProductId });
                }

                // ДОБАВЛЕНИЕ НОВОГО ОТЗЫВА
                var reviewData = new
                {
                    productId = model.ProductId,
                    userId = currentUser.Id_User,
                    rating = model.Rating,
                    commentRe = model.CommentRe
                };

                var json = JsonSerializer.Serialize(reviewData);
                Console.WriteLine($"📤 Данные: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var apiUrl = $"{_apiBaseUrl}reviews";

                Console.WriteLine($"🌐 URL: {apiUrl}");
                var response = await _httpClient.PostAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Ответ: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "✅ Ваш отзыв успешно добавлен!";

                    await Task.Delay(100); 
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseContent, _jsonOptions);
                    TempData["Error"] = errorResponse?.Message ?? "Ошибка при добавлении отзыва";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Ошибка: {ex}");
                TempData["Error"] = "Ошибка при добавлении отзыва";
            }

            return RedirectToAction("Details", new { id = model.ProductId });
        }

        public async Task<IActionResult> Featured()
        {
            try
            {
                var url = $"{_apiBaseUrl}products/featured";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductViewModel>>>(json, _jsonOptions);

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                        return View(apiResponse.Data);
                }

                return View(new List<ProductViewModel>());
            }
            catch
            {
                return View(new List<ProductViewModel>());
            }
        }

        private async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            try
            {
                var url = $"{_apiBaseUrl}categories";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<CategoryViewModel>();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CategoryViewModel>>>(json, _jsonOptions);

                return apiResponse?.Success == true && apiResponse.Data != null
                    ? apiResponse.Data
                    : new List<CategoryViewModel>();
            }
            catch
            {
                return new List<CategoryViewModel>();
            }
        }

        private async Task<ProductListViewModel> BuildEmptyListModel(int? categoryId, string? search)
        {
            return new ProductListViewModel
            {
                Products = new List<ProductViewModel>(),
                Categories = await GetCategoriesAsync(),
                CurrentPage = 1,
                TotalPages = 1,
                TotalCount = 0,
                SelectedCategoryId = categoryId ?? 0,
                SearchTerm = search ?? ""
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int productId, int userId)
        {
            try
            {
                Console.WriteLine($"=== САЙТ: УДАЛЕНИЕ ОТЗЫВА ===");
                Console.WriteLine($"Product: {productId}, User: {userId}");

                var currentUser = GetUserFromCookie();
                if (currentUser?.Id_User != userId)
                {
                    TempData["Error"] = "Можно удалять только свои отзывы";
                    return RedirectToAction("Details", new { id = productId });
                }

                var url = $"{_apiBaseUrl}reviews/user/{userId}/product/{productId}";
                Console.WriteLine($"🌐 DELETE запрос: {url}");

                var response = await _httpClient.DeleteAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📥 Ответ DELETE: {response.StatusCode}");
                Console.WriteLine($"📥 Содержимое: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "✅ Ваш отзыв успешно удален!";

                    Console.WriteLine("⏳ Ожидание синхронизации БД...");
                    await Task.Delay(1000);
                    Console.WriteLine("✅ Синхронизация завершена");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseContent, _jsonOptions);
                    TempData["Error"] = errorResponse?.Message ?? "Ошибка при удалении отзыва";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPTION на сайте: {ex}");
                TempData["Error"] = "Ошибка при удалении отзыва. Попробуйте позже.";
            }

            return RedirectToAction("Details", new { id = productId });
        }
    }
}