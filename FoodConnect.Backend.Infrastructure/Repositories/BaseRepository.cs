using AutoMapper;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static Amazon.S3.Util.S3EventNotification;
using System.Linq.Expressions;
using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        protected readonly IMapper? _mapper;
        public BaseRepository(AppDbContext context, IMapper? mapper = null)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _mapper = mapper;
        }

        public async Task AddAsync(T entity)
        {
            _context.Set<T>().Entry(entity).State = EntityState.Added;
            await _context.Set<T>().AddAsync(entity);
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _context.Set<T>().Entry(entity).State = EntityState.Added;
            }
            await _context.Set<T>().AddRangeAsync(entities);
        }
        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();
        public async Task<T?> GetByIdAsync(Guid id,
        params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }
        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }
        public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return Enumerable.Empty<T>();

            return await _dbSet
                .Where(e => ids.Contains(EF.Property<Guid>(e, "Id")))
                .ToListAsync();
        }
        public void Remove(T entity)
        {
            _context.Set<T>().Entry(entity).State = EntityState.Deleted;
            _context.Set<T>().Remove(entity);
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _context.Set<T>().Entry(entity).State = EntityState.Deleted;
            }
            _context.Set<T>().RemoveRange(entities);
        }
        public void Update(T entity)
        {
            _context.Set<T>().Entry(entity).State = EntityState.Modified;
            _context.Set<T>().Update(entity);
        }
    }
}
