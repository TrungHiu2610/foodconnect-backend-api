using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
        
        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .ToListAsync();
        }
        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive) 
                .Include(c => c.Parent) 
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetParentCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && c.ParentId == null)  // Only parent categories
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithDetailsAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();
        }
    }
}
