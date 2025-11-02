using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetParentCategoriesAsync();
        Task<IEnumerable<Category>> GetAllCategoriesWithDetailsAsync();
        Task<bool> HasChildrenAsync(Guid categoryId);
        Task<IEnumerable<Category>> GetChildrenByParentIdAsync(Guid parentId);
        Task<IEnumerable<Category>> GetCategoriesByIdsAsync(List<Guid> categoryIds);
    }
}
