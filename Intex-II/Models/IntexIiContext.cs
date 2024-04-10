using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Intex_II.Models;

public partial class IntexIiContext : DbContext
{
    public IntexIiContext()
    {
    }

    public IntexIiContext(DbContextOptions<IntexIiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<LineItem> LineItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:intex.database.windows.net,1433;Initial Catalog=Intex-II;Persist Security Info=False;User ID=IntexAdmin;Password=2n3LBLv@4ERPpzH;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasDefaultValue("");
            entity.Property(e => e.LastName).HasDefaultValue("");
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => new { e.CustomerId, e.ProductId });

            entity.ToTable("Cart");

            entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
            entity.Property(e => e.ItemQuantity).HasColumnName("Item_Quantity");
            entity.Property(e => e.TotalPrice).HasColumnName("Total_Price");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerId)
                .ValueGeneratedNever()
                .HasColumnName("Customer_Id");
            entity.Property(e => e.BirthDate).HasColumnName("Birth_Date");
            entity.Property(e => e.CustomerAge).HasColumnName("Customer_Age");
            entity.Property(e => e.CustomerCountry).HasColumnName("Customer_Country");
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(50)
                .HasColumnName("Customer_Email");
            entity.Property(e => e.CustomerFname).HasColumnName("Customer_FName");
            entity.Property(e => e.CustomerGender)
                .HasMaxLength(1)
                .HasColumnName("Customer_Gender");
            entity.Property(e => e.CustomerLname).HasColumnName("Customer_LName");
        });

        modelBuilder.Entity<LineItem>(entity =>
        {
            entity.HasKey(e => new { e.TransactionId, e.ProductId });

            entity.Property(e => e.TransactionId).HasColumnName("Transaction_Id");
            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK_Products_Transaction_Id");

            entity.Property(e => e.TransactionId).HasColumnName("Transaction_Id");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
            entity.Property(e => e.EntryMode).HasColumnName("Entry_Mode");
            entity.Property(e => e.TransactionBank).HasColumnName("Transaction_Bank");
            entity.Property(e => e.TransactionCountry).HasColumnName("Transaction_Country");
            entity.Property(e => e.TransactionDate).HasColumnName("Transaction_Date");
            entity.Property(e => e.TransactionDayOfWeek).HasColumnName("Transaction_Day_Of_Week");
            entity.Property(e => e.TransactionShippingAddress).HasColumnName("Transaction_Shipping_Address");
            entity.Property(e => e.TransactionTime).HasColumnName("Transaction_Time");
            entity.Property(e => e.TransactionType).HasColumnName("Transaction_Type");
            entity.Property(e => e.TransactionTypeOfCard).HasColumnName("Transaction_Type_Of_Card");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
            entity.Property(e => e.ProductCategory).HasColumnName("Product_Category");
            entity.Property(e => e.ProductCategorySimple).HasColumnName("Product_Category_Simple");
            entity.Property(e => e.ProductDescription).HasColumnName("Product_Description");
            entity.Property(e => e.ProductImage).HasColumnName("Product_Image");
            entity.Property(e => e.ProductName).HasColumnName("Product_Name");
            entity.Property(e => e.ProductNumParts).HasColumnName("Product_Num_Parts");
            entity.Property(e => e.ProductPrice).HasColumnName("Product_Price");
            entity.Property(e => e.ProductPrimaryColor).HasColumnName("Product_Primary_Color");
            entity.Property(e => e.ProductSecondaryColor).HasColumnName("Product_Secondary_Color");
            entity.Property(e => e.ProductYear).HasColumnName("Product_Year");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
