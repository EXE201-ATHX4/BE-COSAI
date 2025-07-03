using Contract.Repositories.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Base
{
    public class ComesticsSalesDBContext : IdentityDbContext<User, ApplicationRole, int, ApplicationUserClaims, ApplicationUserRoles, ApplicationUserLogins, ApplicationRoleClaims, ApplicationUserTokens>
    {
        public ComesticsSalesDBContext(DbContextOptions<ComesticsSalesDBContext> options) : base(options) { }
        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
        public virtual DbSet<ApplicationUserClaims> UserClaims => Set<ApplicationUserClaims>();
        public virtual DbSet<ApplicationUserRoles> UserRoles => Set<ApplicationUserRoles>();
        public virtual DbSet<ApplicationUserLogins> UserLogins => Set<ApplicationUserLogins>();
        public virtual DbSet<ApplicationRoleClaims> RoleClaims => Set<ApplicationRoleClaims>();
        public virtual DbSet<ApplicationUserTokens> UserTokens => Set<ApplicationUserTokens>();
        public DbSet<Order> Orders { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShippingMethod> ShippingMethods { get; set; }
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableAnnotation = entityType.GetAnnotation("Relational:TableName");
                string tableName = tableAnnotation?.Value?.ToString() ?? "";
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }
            modelBuilder.Entity<Category>().Property(a => a.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Order>().Property(w => w.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderDetail>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Product>().Property(n => n.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Review>().Property(nw => nw.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Shipment>().Property(m => m.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ShipmentStatusHistory>().Property(mg => mg.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ShippingAddress>().Property(hr => hr.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Supplier>().Property(b => b.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Brand>().Property(b => b.Id).ValueGeneratedOnAdd();
        }
    }
}
