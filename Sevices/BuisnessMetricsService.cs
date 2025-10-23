using Prometheus;
using CosmeticShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticShopAPI.Services
{
    public interface IBusinessMetricsService
    {
        void UpdateMetrics();
    }

    public class BusinessMetricsService : IBusinessMetricsService
    {
        private readonly CosmeticsShopDbContext _context;
        private readonly ILogger<BusinessMetricsService> _logger;

        private static readonly Gauge _usersByRole = Metrics.CreateGauge(
            "cosmeticshop_users_by_role",
            "Количество пользователей по ролям",
            new GaugeConfiguration
            {
                LabelNames = new[] { "role" }
            });

        private static readonly Gauge _productsByCategory = Metrics.CreateGauge(
            "cosmeticshop_products_by_category",
            "Количество товаров по категориям",
            new GaugeConfiguration
            {
                LabelNames = new[] { "category" }
            });

        private static readonly Gauge _ordersByUser = Metrics.CreateGauge(
            "cosmeticshop_orders_by_user",
            "Количество заказов по пользователям",
            new GaugeConfiguration
            {
                LabelNames = new[] { "userId" }
            });

        public BusinessMetricsService(CosmeticsShopDbContext context, ILogger<BusinessMetricsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void UpdateMetrics()
        {
            try
            {
                UpdateUserMetrics();
                UpdateProductMetrics();
                UpdateOrderMetrics();

                _logger.LogInformation("Business metrics updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating business metrics");
            }
        }

        private void UpdateUserMetrics()
        {
            var usersByRole = _context.Users
                .GroupBy(u => u.RoleUs)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToList();

            foreach (var group in usersByRole)
            {
                _usersByRole.WithLabels(group.Role ?? "unknown").Set(group.Count);
            }
        }

        private void UpdateProductMetrics()
        {
            var categories = _context.Categories
                .Select(c => new { c.Id_Category, c.NameCa })
                .ToList();

            var productsByCategory = _context.Products
                .Where(p => p.CategoryID != 0)
                .GroupBy(p => p.CategoryID)
                .Select(g => new { CategoryID = g.Key, Count = g.Count() })
                .ToList();

            foreach (var group in productsByCategory)
            {
                var category = categories.FirstOrDefault(c => c.Id_Category == group.CategoryID);
                var categoryName = category?.NameCa ?? "Без категории";
                _productsByCategory.WithLabels(categoryName).Set(group.Count);
            }
        }

        private void UpdateOrderMetrics()
        {
            var ordersByUser = _context.Orders
                .GroupBy(o => o.UserID)
                .Select(g => new { UserId = g.Key.ToString(), Count = g.Count() })
                .ToList();

            foreach (var group in ordersByUser)
            {
                _ordersByUser.WithLabels(group.UserId).Set(group.Count);
            }
        }
    }
}