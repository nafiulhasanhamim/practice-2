
using System.Security.Cryptography.X509Certificates;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories


{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _ctx;
        public CategoryRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        public IQueryable<Category> GetAllAsync() =>  _ctx.Categories.AsNoTracking();
        public async Task<Category> GetByIdAsync(string categoryId) => await _ctx.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        public async Task AddAsync(Category category)
        {
            await _ctx.Categories.AddAsync(category);
            await _ctx.SaveChangesAsync();
        }
        public async Task UpdateAsync(Category category)
        {
            _ctx.Categories.Update(category);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(string categoryId)
        {
            var category = await _ctx.Categories.FindAsync(categoryId);

            if (category != null)
            {
                _ctx.Categories.Remove(category);
                await _ctx.SaveChangesAsync();
            }
        }

    }

}