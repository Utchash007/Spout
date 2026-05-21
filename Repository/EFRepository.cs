using System;
using Twit.Repository;
using Twit.Models;
using Microsoft.EntityFrameworkCore;
using Twit.Repository.DBContext;
namespace Twit.Repository
{
    public class EFRepository<TEntity>: IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        public EFRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TEntity> Get(string id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task Add(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            // Removed SaveChangesAsync to support UnitOfWork pattern
        }

        public async Task Delete(string id)
        {
            var entity = await Get(id);
            if(entity == null) return;
            _context.Set<TEntity>().Remove(entity);
            // Removed SaveChangesAsync to support UnitOfWork pattern
        }

        public async Task Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            // Removed SaveChangesAsync to support UnitOfWork pattern
            await Task.CompletedTask;
        }

       public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().AsQueryable();
        }
    }
}
