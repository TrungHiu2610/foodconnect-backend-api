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

        public async Task<GetListProductResponse> GetListProductResponseAsync()
        {
            var products = await GetListProductsAsync();
            if(products == null || !products.Any())
            {
                return new GetListProductResponse
                {
                    Products = new List<GetListProductDetail>()
                };
            }
            var response = new GetListProductResponse
            {
                Products = _mapper.Map<List<GetListProductDetail>>(products)
            };

            return response;
        }
        public async Task<IEnumerable<Product>> GetListProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductAssets)
                .Include(p => p.ProductDailyAvailabilities)
                .ToListAsync();
        }   
    }
}
