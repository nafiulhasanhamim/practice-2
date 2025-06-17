using Application.Common.Models;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<PaginatedResult<GetCategoryDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<GetCategoryDto> GetByIdAsync(string categoryId);
        Task<GetCategoryDto> AddAsync(AddCategoryDto addCategoryDto);
        Task UpdateAsync(string categoryId, UpdateCategoryDto updateCategoryDto);
        Task DeleteAsync(string categoryId);
    }
}