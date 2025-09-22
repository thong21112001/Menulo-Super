using Menulo.Domain.Entities;
using Menulo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Infrastructure.Persistence
{
    public class AppDbContext
       : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<RestaurantAdmin> RestaurantAdmins => Set<RestaurantAdmin>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
        public DbSet<ItemsTmp> ItemsTmps => Set<ItemsTmp>();


        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.HasDefaultSchema("dbo");

            // (Tùy chọn) explicit key config cho bảng Identity mặc định
            b.Entity<IdentityUserLogin<string>>().HasKey(p => new { p.LoginProvider, p.ProviderKey });
            b.Entity<IdentityUserRole<string>>().HasKey(p => new { p.UserId, p.RoleId });
            b.Entity<IdentityUserToken<string>>().HasKey(p => new { p.UserId, p.LoginProvider, p.Name });


            // =============== Restaurant ===============
            b.Entity<Restaurant>(cfg =>
            {
                cfg.HasKey(x => x.RestaurantId);

                cfg.Property(x => x.Name).IsRequired().HasMaxLength(200);
                cfg.Property(x => x.Address).HasMaxLength(400);
                cfg.Property(x => x.Phone).HasMaxLength(20);
                cfg.Property(x => x.CreatedAt)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("(getdate())");

                cfg.Property(x => x.StaticQrImageUrl).HasColumnType("varbinary(max)");

                cfg.HasOne<ApplicationUser>()
                   .WithMany()
                   .HasForeignKey(r => r.CreatedBySaleId)
                   .OnDelete(DeleteBehavior.SetNull);
            });


            // =============== RestaurantAdmin (bảng nối User <-> Restaurant) ===============
            b.Entity<RestaurantAdmin>(cfg =>
            {
                cfg.ToTable("RestaurantAdmins");
                cfg.HasKey(x => new { x.RestaurantId, x.UserId });

                cfg.HasOne(x => x.Restaurant)
                   .WithMany(r => r.Admins)
                   .HasForeignKey(x => x.RestaurantId)
                   .OnDelete(DeleteBehavior.Cascade);

                // Liên kết sang ApplicationUser qua UserId (không cần navigation ngược)
                cfg.HasOne<ApplicationUser>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            });


            // =============== Category ===============
            b.Entity<Category>(cfg =>
            {
                cfg.HasKey(x => x.CategoryId);
                cfg.Property(x => x.CategoryName).HasMaxLength(200);
                cfg.HasIndex(x => x.RestaurantId);

                cfg.HasOne(x => x.Restaurant)
                   .WithMany(r => r.Categories)
                   .HasForeignKey(x => x.RestaurantId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

                // Tên danh mục không trùng trong cùng nhà hàng
                cfg.HasIndex(x => new { x.RestaurantId, x.CategoryName }).IsUnique();
            });


            // =============== MenuItem ===============
            b.Entity<MenuItem>(cfg =>
            {
                cfg.HasKey(x => x.ItemId);
                cfg.Property(x => x.ItemName).HasMaxLength(200);
                cfg.Property(x => x.Description).HasMaxLength(1000);
                cfg.Property(x => x.Price).HasColumnType("decimal(18,2)");
                cfg.HasIndex(x => x.RestaurantId);

                cfg.HasOne(x => x.Category)
                   .WithMany(c => c.MenuItems)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

                cfg.HasOne(x => x.Restaurant)
                   .WithMany(r => r.MenuItems)
                   .HasForeignKey(x => x.RestaurantId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

                // Global filter: bỏ các món đã soft-delete (vẫn còn trong CSDL)
                cfg.HasQueryFilter(mi => !mi.IsDeleted);
            });


            // =============== Order ===============
            b.Entity<Order>(cfg =>
            {
                cfg.HasKey(x => x.OrderId);

                cfg.Property(x => x.OrderTime)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("(getdate())");

                cfg.Property(x => x.Status).HasMaxLength(50);
                cfg.Property(x => x.CustomerPhone).HasMaxLength(20);
                cfg.HasIndex(x => x.RestaurantId);

                cfg.HasOne(x => x.Restaurant)
                   .WithMany(r => r.Orders)
                   .HasForeignKey(x => x.RestaurantId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

                cfg.HasOne(x => x.Table)
                   .WithMany(t => t.Orders)
                   .HasForeignKey(x => x.TableId)
                   .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // =============== OrderItem ===============
            b.Entity<OrderItem>(cfg =>
            {
                cfg.HasKey(x => x.OrderItemId);
                cfg.Property(x => x.Price).HasColumnType("decimal(18,2)");

                cfg.HasOne(x => x.Item)
                   .WithMany(mi => mi.OrderItems)
                   .HasForeignKey(x => x.ItemId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

                cfg.HasOne(x => x.Order)
                   .WithMany(o => o.OrderItems)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // =============== RestaurantTable ===============
            b.Entity<RestaurantTable>(cfg =>
            {
                cfg.HasKey(x => x.TableId);
                cfg.Property(x => x.TableCode).HasMaxLength(100);
                cfg.Property(x => x.Description).HasMaxLength(400);
                cfg.HasIndex(x => x.RestaurantId);

                cfg.HasOne(x => x.Restaurant)
                   .WithMany(r => r.RestaurantTables)
                   .HasForeignKey(x => x.RestaurantId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

                // Mã bàn duy nhất trong nhà hàng
                cfg.HasIndex(x => new { x.RestaurantId, x.TableCode }).IsUnique();
            });


            // =============== ItemsTmp (giỏ tạm theo bàn) ===============
            b.Entity<ItemsTmp>(cfg =>
            {
                cfg.HasKey(e => e.ItemsTmpId);
                cfg.ToTable("ItemsTmp");

                cfg.HasOne(e => e.Item)
                      .WithMany(mi => mi.ItemsTmps)
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                cfg.HasOne(e => e.Table)
                      .WithMany(t => t.ItemsTmps)
                      .HasForeignKey(e => e.TableId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                // Chống trùng 1 món nhiều dòng trên cùng bàn
                cfg.HasIndex(e => new { e.TableId, e.ItemId }).IsUnique();
            });
        }
    }
}
