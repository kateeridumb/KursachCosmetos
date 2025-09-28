using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CosmeticShopAPI.Models;

public partial class CosmeticsShopDbContext : DbContext
{
    public CosmeticsShopDbContext()
    {
    }

    public CosmeticsShopDbContext(DbContextOptions<CosmeticsShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<PromoCode> PromoCodes { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<VwProductReview> VwProductReviews { get; set; }

    public virtual DbSet<VwProductStock> VwProductStocks { get; set; }

    public virtual DbSet<VwSalesByCategory> VwSalesByCategories { get; set; }

    public virtual DbSet<VwUserOrder> VwUserOrders { get; set; }


}
