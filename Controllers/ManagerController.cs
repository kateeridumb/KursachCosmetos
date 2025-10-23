using ClosedXML.Excel;
using CosmeticShopWeb.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CosmeticShopWeb.Controllers
{
    public class ManagerController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ManagerController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5094/api/";
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("=== GET Manager/Index вызван ===");

            if (!IsManager())
            {
                Console.WriteLine("Доступ запрещен: пользователь не менеджер");
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index", "Home");
            }

            var model = new ManagerDashboardViewModel();

            try
            {
                model.Products = await GetProductsWithImages();

                model.SalesData = GenerateSalesData();
                model.StockData = GenerateStockData(model.Products);
                model.CategoryData = GenerateCategoryData(model.Products);

                Console.WriteLine($"Загружено товаров: {model.Products.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в Manager/Index: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                TempData["Error"] = "Ошибка при загрузке данных";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsManager())
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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка при удалении товара: {response.StatusCode}, {errorContent}");
                    TempData["Error"] = "Ошибка при удалении товара";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при удалении товара: {ex.Message}");
                TempData["Error"] = "Произошла ошибка при удалении товара";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductStatus(int productId, bool isAvailable)
        {
            if (!IsManager())
            {
                TempData["Error"] = "Доступ запрещен";
                return RedirectToAction("Index");
            }

            try
            {
                var productResponse = await _httpClient.GetAsync($"{_apiBaseUrl}Products/{productId}");
                if (!productResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Товар не найден";
                    return RedirectToAction("Index");
                }

                var productContent = await productResponse.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductApiModel>>(productContent, _jsonOptions);

                if (!apiResponse.Success || apiResponse.Data == null)
                {
                    TempData["Error"] = "Не удалось загрузить данные товара";
                    return RedirectToAction("Index");
                }

                var currentProduct = apiResponse.Data;

                var updateProduct = new
                {
                    CategoryID = currentProduct.CategoryId,
                    NamePr = currentProduct.NamePr,
                    DescriptionPr = currentProduct.DescriptionPr,
                    BrandPr = currentProduct.BrandPr,
                    Price = currentProduct.Price,
                    StockQuantity = currentProduct.StockQuantity,
                    IsAvailable = isAvailable
                };

                var json = JsonSerializer.Serialize(updateProduct);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}Products/{productId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Статус товара обновлен";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка при обновлении статуса: {response.StatusCode}, {errorContent}");
                    TempData["Error"] = "Ошибка при обновлении статуса товара";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при обновлении статуса: {ex.Message}");
                TempData["Error"] = "Произошла ошибка при обновлении статуса товара";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel()
        {
            if (!IsManager())
            {
                return Forbid();
            }

            try
            {
                var products = await GetProductsWithImages();
                var salesData = GenerateSalesData();
                var stockData = GenerateStockData(products);
                var categoryData = GenerateCategoryData(products);

                using (var workbook = new XLWorkbook())
                {
                    var productsWorksheet = workbook.Worksheets.Add("Товары");
                    productsWorksheet.Cell(1, 1).Value = "Отчет по товарам";
                    productsWorksheet.Range(1, 1, 1, 8).Merge().Style.Font.Bold = true;

                    var headers = new[] { "ID", "Название", "Бренд", "Цена", "Категория", "Остаток", "Статус", "Дата создания" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        productsWorksheet.Cell(3, i + 1).Value = headers[i];
                        productsWorksheet.Cell(3, i + 1).Style.Font.Bold = true;
                    }

                    int row = 4;
                    foreach (var product in products)
                    {
                        productsWorksheet.Cell(row, 1).Value = product.IdProduct;
                        productsWorksheet.Cell(row, 2).Value = product.NamePr;
                        productsWorksheet.Cell(row, 3).Value = product.BrandPr;
                        productsWorksheet.Cell(row, 4).Value = product.Price;
                        productsWorksheet.Cell(row, 5).Value = product.CategoryName;
                        productsWorksheet.Cell(row, 6).Value = product.StockQuantity;
                        productsWorksheet.Cell(row, 7).Value = product.IsAvailable ? "Активен" : "Неактивен";
                        productsWorksheet.Cell(row, 8).Value = DateTime.Now.ToString("dd.MM.yyyy");
                        row++;
                    }

                    var analyticsWorksheet = workbook.Worksheets.Add("Аналитика");

                    analyticsWorksheet.Cell(1, 1).Value = "Продажи по месяцам";
                    analyticsWorksheet.Range(1, 1, 1, 3).Merge().Style.Font.Bold = true;

                    analyticsWorksheet.Cell(2, 1).Value = "Месяц";
                    analyticsWorksheet.Cell(2, 2).Value = "Продажи (руб)";
                    analyticsWorksheet.Cell(2, 3).Value = "Рост %";
                    analyticsWorksheet.Range(2, 1, 2, 3).Style.Font.Bold = true;

                    for (int i = 0; i < salesData.Labels.Count; i++)
                    {
                        analyticsWorksheet.Cell(i + 3, 1).Value = salesData.Labels[i];
                        analyticsWorksheet.Cell(i + 3, 2).Value = salesData.Values[i];

                        if (i > 0)
                        {
                            var growth = ((salesData.Values[i] - salesData.Values[i - 1]) / salesData.Values[i - 1] * 100);
                            analyticsWorksheet.Cell(i + 3, 3).Value = Math.Round(growth, 2);
                        }
                    }

                    int stockRow = salesData.Labels.Count + 5;
                    analyticsWorksheet.Cell(stockRow, 1).Value = "Остатки на складе (топ-6)";
                    analyticsWorksheet.Range(stockRow, 1, stockRow, 3).Merge().Style.Font.Bold = true;

                    analyticsWorksheet.Cell(stockRow + 1, 1).Value = "Товар";
                    analyticsWorksheet.Cell(stockRow + 1, 2).Value = "Количество";
                    analyticsWorksheet.Range(stockRow + 1, 1, stockRow + 1, 2).Style.Font.Bold = true;

                    for (int i = 0; i < stockData.Labels.Count; i++)
                    {
                        analyticsWorksheet.Cell(stockRow + 2 + i, 1).Value = stockData.Labels[i];
                        analyticsWorksheet.Cell(stockRow + 2 + i, 2).Value = stockData.Values[i];
                    }

                    int categoryRow = stockRow + stockData.Labels.Count + 4;
                    analyticsWorksheet.Cell(categoryRow, 1).Value = "Товары по категориям";
                    analyticsWorksheet.Range(categoryRow, 1, categoryRow, 3).Merge().Style.Font.Bold = true;

                    analyticsWorksheet.Cell(categoryRow + 1, 1).Value = "Категория";
                    analyticsWorksheet.Cell(categoryRow + 1, 2).Value = "Количество товаров";
                    analyticsWorksheet.Range(categoryRow + 1, 1, categoryRow + 1, 2).Style.Font.Bold = true;

                    for (int i = 0; i < categoryData.Labels.Count; i++)
                    {
                        analyticsWorksheet.Cell(categoryRow + 2 + i, 1).Value = categoryData.Labels[i];
                        analyticsWorksheet.Cell(categoryRow + 2 + i, 2).Value = categoryData.Values[i];
                    }

                    int statsRow = categoryRow + categoryData.Labels.Count + 4;
                    analyticsWorksheet.Cell(statsRow, 1).Value = "Общая статистика";
                    analyticsWorksheet.Range(statsRow, 1, statsRow, 3).Merge().Style.Font.Bold = true;

                    analyticsWorksheet.Cell(statsRow + 1, 1).Value = "Всего товаров";
                    analyticsWorksheet.Cell(statsRow + 1, 2).Value = products.Count;

                    analyticsWorksheet.Cell(statsRow + 2, 1).Value = "Активных товаров";
                    analyticsWorksheet.Cell(statsRow + 2, 2).Value = products.Count(p => p.IsAvailable);

                    analyticsWorksheet.Cell(statsRow + 3, 1).Value = "Неактивных товаров";
                    analyticsWorksheet.Cell(statsRow + 3, 2).Value = products.Count(p => !p.IsAvailable);

                    analyticsWorksheet.Cell(statsRow + 4, 1).Value = "Товаров с малым остатком";
                    analyticsWorksheet.Cell(statsRow + 4, 2).Value = products.Count(p => p.StockQuantity < 10);

                    productsWorksheet.Columns().AdjustToContents();
                    analyticsWorksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Отчет_менеджера_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при экспорте в Excel: {ex.Message}");
                TempData["Error"] = "Ошибка при экспорте в Excel";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPDF()
        {
            if (!IsManager())
            {
                return Forbid();
            }

            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var products = await GetProductsWithImages();
                var salesData = GenerateSalesData();
                var stockData = GenerateStockData(products);
                var categoryData = GenerateCategoryData(products);

                using (var stream = new MemoryStream())
                {
                    var doc = new Document(PageSize.A4, 25, 25, 30, 30);
                    var writer = PdfWriter.GetInstance(doc, stream);
                    writer.CloseStream = false;

                    BaseFont baseFont;
                    try
                    {
                        baseFont = BaseFont.CreateFont(
                            "c:/windows/fonts/arial.ttf",
                            BaseFont.IDENTITY_H,
                            BaseFont.EMBEDDED
                        );
                    }
                    catch (Exception fontEx)
                    {
                        Console.WriteLine($"Ошибка загрузки шрифта Arial: {fontEx.Message}");

                        baseFont = BaseFont.CreateFont(
                            BaseFont.HELVETICA,
                            BaseFont.IDENTITY_H,
                            BaseFont.EMBEDDED
                        );
                    }

                    var font = new Font(baseFont, 12);
                    var titleFont = new Font(baseFont, 16, Font.BOLD);
                    var headerFont = new Font(baseFont, 12, Font.BOLD);

                    doc.Open();

                    doc.Add(new Paragraph("ОТЧЕТ МЕНЕДЖЕРА", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 20f
                    });

                    doc.Add(new Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}", font)
                    {
                        Alignment = Element.ALIGN_RIGHT,
                        SpacingAfter = 20f
                    });

                    // Общая статистика
                    doc.Add(new Paragraph("ОБЩАЯ СТАТИСТИКА", headerFont));

                    var statsTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100,
                        SpacingAfter = 20f
                    };

                    AddPdfCell(statsTable, "Всего товаров", font);
                    AddPdfCell(statsTable, products.Count.ToString(), font);
                    AddPdfCell(statsTable, "Активных товаров", font);
                    AddPdfCell(statsTable, products.Count(p => p.IsAvailable).ToString(), font);
                    AddPdfCell(statsTable, "Неактивных товаров", font);
                    AddPdfCell(statsTable, products.Count(p => !p.IsAvailable).ToString(), font);
                    AddPdfCell(statsTable, "Товаров с малым остатком", font);
                    AddPdfCell(statsTable, products.Count(p => p.StockQuantity < 10).ToString(), font);

                    doc.Add(statsTable);

                    doc.Add(new Paragraph("ПРОДАЖИ ПО МЕСЯЦАМ", headerFont));

                    var salesTable = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        SpacingAfter = 20f
                    };

                    AddPdfCell(salesTable, "Месяц", headerFont);
                    AddPdfCell(salesTable, "Продажи (руб)", headerFont);
                    AddPdfCell(salesTable, "Рост %", headerFont);

                    for (int i = 0; i < salesData.Labels.Count; i++)
                    {
                        AddPdfCell(salesTable, salesData.Labels[i], font);
                        AddPdfCell(salesTable, salesData.Values[i].ToString("N2"), font);

                        if (i > 0)
                        {
                            var growth = ((salesData.Values[i] - salesData.Values[i - 1]) / salesData.Values[i - 1] * 100);
                            AddPdfCell(salesTable, growth.ToString("N2") + "%", font);
                        }
                        else
                        {
                            AddPdfCell(salesTable, "-", font);
                        }
                    }

                    doc.Add(salesTable);

                    doc.Add(new Paragraph("ОСТАТКИ НА СКЛАДЕ (ТОП-6)", headerFont));

                    var stockTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100,
                        SpacingAfter = 20f
                    };

                    AddPdfCell(stockTable, "Товар", headerFont);
                    AddPdfCell(stockTable, "Количество", headerFont);

                    for (int i = 0; i < stockData.Labels.Count; i++)
                    {
                        AddPdfCell(stockTable, stockData.Labels[i], font);
                        AddPdfCell(stockTable, stockData.Values[i].ToString(), font);
                    }

                    doc.Add(stockTable);

                    doc.Add(new Paragraph("ТОВАРЫ ПО КАТЕГОРИЯМ", headerFont));

                    var categoryTable = new PdfPTable(2)
                    {
                        WidthPercentage = 100,
                        SpacingAfter = 20f
                    };

                    AddPdfCell(categoryTable, "Категория", headerFont);
                    AddPdfCell(categoryTable, "Количество товаров", headerFont);

                    for (int i = 0; i < categoryData.Labels.Count; i++)
                    {
                        AddPdfCell(categoryTable, categoryData.Labels[i], font);
                        AddPdfCell(categoryTable, categoryData.Values[i].ToString(), font);
                    }

                    doc.Add(categoryTable);

                    doc.Close();
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/pdf",
                        $"Отчет_менеджера_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при экспорте в PDF: {ex.Message}");
                TempData["Error"] = $"Ошибка при экспорте в PDF: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private void AddPdfCell(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 5,
                BorderWidth = 1
            };
            table.AddCell(cell);
        }


        private async Task<List<ProductViewModel>> GetProductsWithImages()
        {
            try
            {
                var productsResponse = await _httpClient.GetAsync($"{_apiBaseUrl}Products?page=1&pageSize=1000");

                if (!productsResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ошибка API при получении товаров: {productsResponse.StatusCode}");
                    return new List<ProductViewModel>();
                }

                var productsContent = await productsResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"📦 Ответ API продуктов: {productsContent}");

                var productsApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductApiModel>>>(productsContent, _jsonOptions);

                if (!productsApiResponse.Success || productsApiResponse.Data == null)
                {
                    Console.WriteLine("API вернуло неуспешный ответ или пустые данные товаров");
                    return new List<ProductViewModel>();
                }

                var productsData = productsApiResponse.Data;

                var allImages = await GetAllImages();
                var productViewModels = new List<ProductViewModel>();

                foreach (var productData in productsData)
                {
                    var productImages = allImages.Where(img => img.ProductID == productData.IdProduct).ToList();
                    var mainImage = productImages.FirstOrDefault();

                    var mainImageUrl = mainImage?.ImageURL ?? "/images/placeholder-product.jpg";
                    if (!mainImageUrl.StartsWith("http") && !mainImageUrl.StartsWith("/"))
                    {
                        mainImageUrl = "/" + mainImageUrl;
                    }

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
                        MainImageUrl = mainImageUrl,
                        ImageUrls = productImages.Select(img =>
                            img.ImageURL.StartsWith("http") || img.ImageURL.StartsWith("/")
                                ? img.ImageURL
                                : "/" + img.ImageURL
                        ).ToList()
                    };

                    productViewModels.Add(productViewModel);
                }

                Console.WriteLine($"✅ Успешно загружено товаров: {productViewModels.Count}");
                return productViewModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в GetProductsWithImages: {ex.Message}");
                return new List<ProductViewModel>();
            }
        }

        private async Task<List<ImageApiModel>> GetAllImages()
        {
            try
            {
                var imagesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}Images");

                if (!imagesResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Ошибка при загрузке изображений: {imagesResponse.StatusCode}");
                    return new List<ImageApiModel>();
                }

                var imagesContent = await imagesResponse.Content.ReadAsStringAsync();
                var imagesApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ImageApiModel>>>(imagesContent, _jsonOptions);

                if (imagesApiResponse?.Success == true && imagesApiResponse.Data != null)
                {
                    return imagesApiResponse.Data;
                }

                return new List<ImageApiModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при загрузке всех изображений: {ex.Message}");
                return new List<ImageApiModel>();
            }
        }

        private SalesChartData GenerateSalesData()
        {
            return new SalesChartData
            {
                Labels = new List<string> { "Янв", "Фев", "Мар", "Апр", "Май", "Июн" },
                Values = new List<decimal> { 120000, 150000, 180000, 140000, 200000, 220000 }
            };
        }

        private StockChartData GenerateStockData(List<ProductViewModel> products)
        {
            var topProducts = products
                .Where(p => p.StockQuantity > 0)
                .OrderByDescending(p => p.StockQuantity)
                .Take(6)
                .ToList();

            return new StockChartData
            {
                Labels = topProducts.Select(p => p.NamePr.Length > 15 ? p.NamePr.Substring(0, 15) + "..." : p.NamePr).ToList(),
                Values = topProducts.Select(p => p.StockQuantity).ToList()
            };
        }

        private CategoryChartData GenerateCategoryData(List<ProductViewModel> products)
        {
            var categories = products
                .GroupBy(p => p.CategoryId)
                .Select(g => new
                {
                    Category = g.First().CategoryName,
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .Take(6)
                .ToList();

            return new CategoryChartData
            {
                Labels = categories.Select(c => c.Category).ToList(),
                Values = categories.Select(c => c.Count).ToList()
            };
        }

        private bool IsManager()
        {
            var user = GetCurrentUser();
            return user?.RoleUs == "Менеджер" || user?.RoleUs == "Администратор";
        }

        private UserLoginResponse GetCurrentUser()
        {
            if (Request.Cookies.TryGetValue("UserAuth", out var encryptedData))
            {
                try
                {
                    var userJson = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData));
                    return JsonSerializer.Deserialize<UserLoginResponse>(userJson, _jsonOptions);
                }
                catch
                {
                    Response.Cookies.Delete("UserAuth");
                    return null;
                }
            }
            return null;
        }
    }

    public class ProductApiModel
    {
        public int IdProduct { get; set; }
        public int CategoryId { get; set; }
        public string NamePr { get; set; }
        public string DescriptionPr { get; set; }
        public string BrandPr { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ImageApiModel
    {
        public int ID_Image { get; set; }
        public int ProductID { get; set; }
        public string ImageURL { get; set; }
        public string DescriptionIMG { get; set; }
    }
}