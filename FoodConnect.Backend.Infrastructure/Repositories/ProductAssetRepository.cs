using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class ProductAssetRepository : BaseRepository<ProductAsset>, IProductAssetRepository
    {
        public ProductAssetRepository(AppDbContext context) : base(context)
        {
        }
    }
}
