using Microsoft.AspNetCore.Mvc;
using CosmeticShopWeb.Models;
using System.Text.Json;

namespace CosmeticShopWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
                // Получаем продукт
                var url = $"{_apiBaseUrl}products/{id}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductViewModel>>(json, _jsonOptions);

                if (apiResponse?.Success != true || apiResponse.Data == null)
                    return NotFound();

                var product = apiResponse.Data;

                // Подгрузка изображений
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

                // Связанные товары
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
    }
}
