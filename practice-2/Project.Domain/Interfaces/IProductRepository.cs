
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProductRepository
    {
        IQueryable<Product> GetAllAsync();
        Task<Product> GetByIdAsync(string productId);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(string productId);
    }
}