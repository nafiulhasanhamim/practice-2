
using Application.Common.Models;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<PaginatedResult<GetProductDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<GetProductDto> GetByIdAsync(string productId);
        Task<GetProductDto> AddAsync(AddProductDto addProductDto);
        Task<GetProductDto> UpdateAsync(string productId, UpdateProductDto updateProductDto);
        Task DeleteAsync(string productId);
    }
}