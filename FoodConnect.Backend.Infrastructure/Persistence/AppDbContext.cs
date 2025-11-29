using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        #region DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAsset> ProductAssets { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<ShopOperatingHour> ShopOperatingHours { get; set; }
        public DbSet<ShopAsset> ShopAssets { get; set; }
        public DbSet<ShopCategory> ShopCategories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductReviewAsset> ProductReviewAssets { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }
        public DbSet<UserStatusAuditLog> UserStatusAuditLogs { get; set; }
        public DbSet<OrderComplaint> OrderComplaints { get; set; }
        public DbSet<OrderComplaintAsset> OrderComplaintAssets { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        #endregion

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var isHardDelete = entityType.ClrType
                        .GetInterfaces()
                        .Any(i => i.Name == nameof(Domain.Interfaces.IHardDelete));

                    if (!isHardDelete)
                    {
                        var method = typeof(AppDbContext)
                            .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                            .MakeGenericMethod(entityType.ClrType);

                        method.Invoke(null, new object[] { modelBuilder });
                    }
                }
            }
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService?.UserId; 
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                }
                
                if (entry.State == EntityState.Deleted)
                {
                    var isHardDelete = entry.Entity.GetType()
                        .GetInterfaces()
                        .Any(i => i.Name == nameof(Domain.Interfaces.IHardDelete));

                    if (!isHardDelete)
                    {
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = userId;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : BaseEntity
        {
            builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
