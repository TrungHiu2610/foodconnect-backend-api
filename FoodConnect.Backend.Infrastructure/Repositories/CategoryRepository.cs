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

        public async Task<GetListCategoryResponse> GetListCategoryResponseAsync()
        {
            var categories = await GetAllAsync();
            if (categories == null || !categories.Any())
            {
                return new GetListCategoryResponse
                {
                    Categories = new List<GetListCategoryDetail>()
                };
            }
            var response = new GetListCategoryResponse
            {
                Categories = _mapper.Map<List<GetListCategoryDetail>>(categories)
            };
            return response;
        }
    }
}
