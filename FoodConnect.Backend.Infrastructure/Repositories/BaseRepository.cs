using AutoMapper;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly IMapper? _mapper;
        public BaseRepository(AppDbContext context, IMapper? mapper = null)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);
        public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();
        public async Task<T?> GetByIdAsync(Guid id) => await _context.Set<T>().FindAsync(id);
        public void Remove(T entity) => _context.Set<T>().Remove(entity);
        public void Update(T entity) => _context.Set<T>().Update(entity);
    }
}
