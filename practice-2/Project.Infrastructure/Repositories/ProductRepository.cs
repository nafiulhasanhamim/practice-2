
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<Product> GetAllAsync() => _db.Products.AsNoTracking().Include(p => p.Category);

        public async Task<Product> GetByIdAsync(string productId) => await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);

        public async Task AddAsync(Product product)
        {
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
        }
        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string productId)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
        }

    }
}