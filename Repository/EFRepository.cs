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
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(string id){
            var entity = await Get(id);
            if(entity == null) return;
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();     
        }

       public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        
    }
}