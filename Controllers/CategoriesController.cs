using Microsoft.AspNetCore.Mvc;
using CosmeticShopWeb.Models;
using System.Text.Json;

namespace CosmeticShopWeb.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public CategoriesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["BaseUrl"] ?? "https://localhost:5094/api/";
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IActionResult> Index()
        {
            var url = $"{_apiBaseUrl}categories";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return View(new List<CategoryViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CategoryViewModel>>>(json, _jsonOptions);

            return View(apiResponse?.Data ?? new List<CategoryViewModel>());
        }

        public async Task<IActionResult> Details(int id)
        {
            var categoryUrl = $"{_apiBaseUrl}categories/{id}";
            var categoryResponse = await _httpClient.GetAsync(categoryUrl);

            if (!categoryResponse.IsSuccessStatusCode) return NotFound();

            var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
            var categoryApiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryViewModel>>(categoryJson, _jsonOptions);

            if (categoryApiResponse?.Success != true || categoryApiResponse.Data == null)
                return NotFound();

            var productsUrl = $"{_apiBaseUrl}products?categoryId={id}";
            var productsResponse = await _httpClient.GetAsync(productsUrl);

            var products = new List<ProductViewModel>();
            if (productsResponse.IsSuccessStatusCode)
            {
                var productsJson = await productsResponse.Content.ReadAsStringAsync();
                var productsApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductViewModel>>>(productsJson, _jsonOptions);

                if (productsApiResponse?.Success == true && productsApiResponse.Data != null)
                    products = productsApiResponse.Data;
            }

            var viewModel = new CategoryDetailsViewModel
            {
                Category = categoryApiResponse.Data,
                Products = products
            };

            return View(viewModel);
        }
    }
}
