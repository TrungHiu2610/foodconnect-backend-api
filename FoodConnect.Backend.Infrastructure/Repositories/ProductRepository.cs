using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        private readonly IMapper _mapper;

        public ProductRepository(AppDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p=>p.ProductAssets)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p=>p.Id == id);
        }
        public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
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
    }
}
