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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-L575937\\SQLEXPRESS;Database=CosmeticsShopDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("PK__AuditLog__2DBF3395062D769E");

            entity.Property(e => e.IdLog).HasColumnName("ID_Log");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NewData).IsUnicode(false);
            entity.Property(e => e.OldData).IsUnicode(false);
            entity.Property(e => e.TableName)
                .HasMaxLength(130)
                .IsUnicode(false);
            entity.Property(e => e.TimestampMl)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.UserName)
                .HasMaxLength(130)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_Users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory).HasName("PK__Categori__6DB3A68A1D6FFE60");

            entity.ToTable(tb => tb.HasTrigger("trg_Categories_Audit"));

            entity.HasIndex(e => e.NameCa, "UQ__Categori__EE1C60F808A9F970").IsUnique();

            entity.Property(e => e.IdCategory).HasColumnName("ID_Category");
            entity.Property(e => e.DescriptionCa)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NameCa)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.IdImage).HasName("PK__Images__31E45A2AAF7F45F7");

            entity.ToTable(tb => tb.HasTrigger("trg_Images_Audit"));

            entity.Property(e => e.IdImage).HasColumnName("ID_Image");
            entity.Property(e => e.DescriptionImg)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("DescriptionIMG");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ImageURL");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Product).WithMany(p => p.Images)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Images_Products");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("PK__Orders__EC9FA9559F6D8A31");

            entity.ToTable(tb => tb.HasTrigger("trg_Orders_Audit"));

            entity.Property(e => e.IdOrder).HasColumnName("ID_Order");
            entity.Property(e => e.DeliveryAddress)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PromoId).HasColumnName("PromoID");
            entity.Property(e => e.StatusOr)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Promo).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PromoId)
                .HasConstraintName("FK_Orders_PromoCodes");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.IdOrderDetail).HasName("PK__OrderDet__855D4EF5515C6D43");

            entity.ToTable(tb => tb.HasTrigger("trg_OrderDetails_Audit"));

            entity.HasIndex(e => new { e.OrderId, e.ProductId }, "UQ_OrderDetails_Order_Product").IsUnique();

            entity.Property(e => e.IdOrderDetail).HasColumnName("ID_OrderDetail");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetails_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduct).HasName("PK__Products__522DE4969C56D36E");

            entity.ToTable(tb => tb.HasTrigger("trg_Products_Audit"));

            entity.Property(e => e.IdProduct).HasColumnName("ID_Product");
            entity.Property(e => e.BrandPr)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.DescriptionPr)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.NamePr)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        });

        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.HasKey(e => e.IdPromo).HasName("PK__PromoCod__06BC9676CD67203E");

            entity.ToTable(tb => tb.HasTrigger("trg_PromoCodes_Audit"));

            entity.HasIndex(e => e.Code, "UQ__PromoCod__A25C5AA77C7727AC").IsUnique();

            entity.Property(e => e.IdPromo).HasColumnName("ID_Promo");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.IdReview).HasName("PK__Reviews__E39E964753D2EBEE");

            entity.ToTable(tb => tb.HasTrigger("trg_Reviews_Audit"));

            entity.HasIndex(e => new { e.UserId, e.ProductId }, "UQ_Reviews_User_Product").IsUnique();

            entity.Property(e => e.IdReview).HasColumnName("ID_Review");
            entity.Property(e => e.CommentRe)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Products");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__Users__ED4DE44220169768");

            entity.ToTable(tb => tb.HasTrigger("trg_Users_Audit"));

            entity.HasIndex(e => e.Phone, "UQ__Users__5C7E359EFA91BB14").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341E32D29C").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.DateRegistered).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RoleUs)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StatusUs)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Активен");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.IdProfile).HasName("PK__UserProf__F1B3F50C1AA862AE");

            entity.ToTable(tb => tb.HasTrigger("trg_UserProfiles_Audit"));

            entity.HasIndex(e => e.UserId, "UQ__UserProf__1788CCADE27E089E").IsUnique();

            entity.Property(e => e.IdProfile).HasColumnName("ID_Profile");
            entity.Property(e => e.AddressPr)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CityPr)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PostalCodePr)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Preferences)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .HasConstraintName("FK_UserProfiles_Users");
        });

        modelBuilder.Entity<VwProductReview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ProductReviews");

            entity.Property(e => e.АвторОтзыва)
                .HasMaxLength(53)
                .IsUnicode(false)
                .HasColumnName("Автор отзыва");
            entity.Property(e => e.ДатаОтзыва)
                .HasColumnType("datetime")
                .HasColumnName("Дата отзыва");
            entity.Property(e => e.Комментарий)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.НомерОтзыва).HasColumnName("Номер отзыва");
            entity.Property(e => e.Товар)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwProductStock>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ProductStock");

            entity.Property(e => e.КодТовара).HasColumnName("Код товара");
            entity.Property(e => e.НаСкладе).HasColumnName("На складе");
            entity.Property(e => e.НазваниеТовара)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Название товара");
        });

        modelBuilder.Entity<VwSalesByCategory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_SalesByCategory");

            entity.Property(e => e.Категория)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ОбщаяСуммаПродаж)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("Общая сумма продаж");
        });

        modelBuilder.Entity<VwUserOrder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UserOrders");

            entity.Property(e => e.ДатаЗаказа)
                .HasColumnType("datetime")
                .HasColumnName("Дата заказа");
            entity.Property(e => e.НомерЗаказа).HasColumnName("Номер заказа");
            entity.Property(e => e.Промокод)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.СтатусЗаказа)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Статус заказа");
            entity.Property(e => e.СуммаЗаказа)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Сумма заказа");
            entity.Property(e => e.ФиоКлиента)
                .HasMaxLength(152)
                .IsUnicode(false)
                .HasColumnName("ФИО клиента");
            entity.Property(e => e.ЭлектроннаяПочта)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Электронная почта");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
