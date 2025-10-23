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

        private const int PageSize = 6;

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
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductApiModel>>>(json, _jsonOptions);

                if (apiResponse?.Success != true || apiResponse.Data == null)
                {
                    return View(await BuildEmptyListModel(categoryId, search));
                }

                var allImages = await GetAllImages();
                var productsWithImages = await ConvertToProductViewModels(apiResponse.Data, allImages);

                var categories = await GetCategoriesAsync();

                var viewModel = new ProductListViewModel
                {
                    Products = productsWithImages,
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
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductApiModel>>(json, _jsonOptions);

                if (apiResponse?.Success != true || apiResponse.Data == null)
                    return NotFound();

                var allImages = await GetAllImages();
                var productImages = allImages.Where(img => img.ProductID == id).ToList();
                var mainImage = productImages.FirstOrDefault();

                var product = new ProductViewModel
                {
                    IdProduct = apiResponse.Data.IdProduct,
                    CategoryId = apiResponse.Data.CategoryId,
                    NamePr = apiResponse.Data.NamePr,
                    DescriptionPr = apiResponse.Data.DescriptionPr ?? string.Empty,
                    BrandPr = apiResponse.Data.BrandPr ?? string.Empty,
                    Price = apiResponse.Data.Price,
                    StockQuantity = apiResponse.Data.StockQuantity,
                    IsAvailable = apiResponse.Data.IsAvailable,
                    MainImageUrl = mainImage?.ImageURL ?? "/images/placeholder-product.jpg",
                    ImageUrls = productImages.Select(img => img.ImageURL).ToList()
                };

                await LoadProductReviews(product);

                try
                {
                    var relatedUrl = $"{_apiBaseUrl}products?categoryId={product.CategoryId}&pageSize=4";
                    var relatedResponse = await _httpClient.GetAsync(relatedUrl);

                    if (relatedResponse.IsSuccessStatusCode)
                    {
                        var relatedJson = await relatedResponse.Content.ReadAsStringAsync();
                        var relatedApi = JsonSerializer.Deserialize<ApiResponse<List<ProductApiModel>>>(relatedJson, _jsonOptions);

                        if (relatedApi?.Success == true && relatedApi.Data != null)
                        {
                            var relatedImages = await GetAllImages();
                            product.RelatedProducts = await ConvertToProductViewModels(
                                relatedApi.Data.Where(p => p.IdProduct != product.IdProduct).ToList(),
                                relatedImages
                            );
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

        public async Task<IActionResult> Featured()
        {
            try
            {
                var url = $"{_apiBaseUrl}products/featured";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductApiModel>>>(json, _jsonOptions);

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        var allImages = await GetAllImages();
                        var productsWithImages = await ConvertToProductViewModels(apiResponse.Data, allImages);
                        return View(productsWithImages);
                    }
                }

                return View(new List<ProductViewModel>());
            }
            catch
            {
                return View(new List<ProductViewModel>());
            }
        }


        private async Task<List<ProductViewModel>> ConvertToProductViewModels(List<ProductApiModel> products, List<ImageApiModel> allImages)
        {
            var productViewModels = new List<ProductViewModel>();

            foreach (var productData in products)
            {
                var productImages = allImages.Where(img => img.ProductID == productData.IdProduct).ToList();
                var mainImage = productImages.FirstOrDefault();

                var productViewModel = new ProductViewModel
                {
                    IdProduct = productData.IdProduct,
                    CategoryId = productData.CategoryId,
                    NamePr = productData.NamePr,
                    DescriptionPr = productData.DescriptionPr ?? string.Empty,
                    BrandPr = productData.BrandPr ?? string.Empty,
                    Price = productData.Price,
                    StockQuantity = productData.StockQuantity,
                    IsAvailable = productData.IsAvailable,
                    MainImageUrl = mainImage?.ImageURL ?? "/images/placeholder-product.jpg",
                    ImageUrls = productImages.Select(img => img.ImageURL).ToList()
                };

                productViewModels.Add(productViewModel);
            }

            return productViewModels;
        }

        private async Task<List<ImageApiModel>> GetAllImages()
        {
            try
            {
                var imagesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}Images");

                if (!imagesResponse.IsSuccessStatusCode)
                    return new List<ImageApiModel>();

                var imagesContent = await imagesResponse.Content.ReadAsStringAsync();
                var imagesApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ImageApiModel>>>(imagesContent, _jsonOptions);

                return imagesApiResponse?.Success == true ? imagesApiResponse.Data : new List<ImageApiModel>();
            }
            catch (Exception)
            {
                return new List<ImageApiModel>();
            }
        }


        private async Task LoadProductReviews(ProductViewModel product)
        {
            try
            {
                var reviewsUrl = $"{_apiBaseUrl}reviews/product/{product.IdProduct}";
                var reviewsResponse = await _httpClient.GetAsync(reviewsUrl);

                if (reviewsResponse.IsSuccessStatusCode)
                {
                    var reviewsJson = await reviewsResponse.Content.ReadAsStringAsync();
                    var reviewsApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ReviewViewModel>>>(reviewsJson, _jsonOptions);

                    if (reviewsApiResponse?.Success == true && reviewsApiResponse.Data != null)
                    {
                        product.Reviews = reviewsApiResponse.Data;
                        product.TotalReviews = product.Reviews.Count;
                        product.AverageRating = product.Reviews.Any() ?
                            Math.Round(product.Reviews.Average(r => r.Rating), 1) : 0;
                    }
                    else
                    {
                        SetEmptyReviews(product);
                    }
                }
                else
                {
                    SetEmptyReviews(product);
                }
            }
            catch (Exception)
            {
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
                var hasExistingReview = await UserHasReviewForProduct(currentUser.Id_User, model.ProductId);

                if (hasExistingReview)
                {
                    TempData["Error"] = "Вы уже оставляли отзыв на этот товар";
                    return RedirectToAction("Details", new { id = model.ProductId });
                }

                var reviewData = new
                {
                    productId = model.ProductId,
                    userId = currentUser.Id_User,
                    rating = model.Rating,
                    commentRe = model.CommentRe
                };

                var json = JsonSerializer.Serialize(reviewData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var apiUrl = $"{_apiBaseUrl}reviews";

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "✅ Ваш отзыв успешно добавлен!";
                    await Task.Delay(100);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<string>>(errorContent, _jsonOptions);
                    TempData["Error"] = errorResponse?.Message ?? "Ошибка при добавлении отзыва";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Ошибка при добавлении отзыва";
            }

            return RedirectToAction("Details", new { id = model.ProductId });
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
                var currentUser = GetUserFromCookie();
                if (currentUser?.Id_User != userId)
                {
                    TempData["Error"] = "Можно удалять только свои отзывы";
                    return RedirectToAction("Details", new { id = productId });
                }

                var url = $"{_apiBaseUrl}reviews/user/{userId}/product/{productId}";
                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "✅ Ваш отзыв успешно удален!";
                    await Task.Delay(1000);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<string>>(errorContent, _jsonOptions);
                    TempData["Error"] = errorResponse?.Message ?? "Ошибка при удалении отзыва";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Ошибка при удалении отзыва. Попробуйте позже.";
            }

            return RedirectToAction("Details", new { id = productId });
        }
    }
}