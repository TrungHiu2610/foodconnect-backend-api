using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p=>p.ProductAssets)
                .Include(p => p.Category)
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p=>p.Id == id);
        }
        public IQueryable<Product> GetProductsAsQueryable()
        {
            return _context.Products.AsQueryable();
        }

        public async Task<Product?> GetProductWithAssetsAsync(Guid id, bool tracking = true)
        {
            var query = _context.Products
                .Include(p => p.ProductAssets)
                .AsQueryable();
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedByShopIdAsync(
            Guid shopId,
            int page,
            int pageSize,
            ProductStatusEnum? status = null,
            string? searchTerm = null)
        {
            var query = _context.Products
                .Include(p => p.ProductAssets)
                .Include(p => p.Category)
                .Include(p => p.Shop)
                .Where(p => p.ShopId == shopId)
                .AsQueryable();

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            // Search by product name or description
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
