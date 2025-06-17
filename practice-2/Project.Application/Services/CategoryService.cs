
using Application.Common.Models;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services

{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<PaginatedResult<GetCategoryDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _categoryRepository.GetAllAsync();
            var total = await query.CountAsync();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(c => new GetCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description
            });

            return new PaginatedResult<GetCategoryDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<GetCategoryDto> GetByIdAsync(string categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category == null)
                return null;

            return new GetCategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<GetCategoryDto> AddAsync(AddCategoryDto addCategoryDto)
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid().ToString(),
                Name = addCategoryDto.Name,
                Description = addCategoryDto.Description
            };

            await _categoryRepository.AddAsync(category);

            return new GetCategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task UpdateAsync(string categoryId, UpdateCategoryDto updateCategoryDto)
        {
            var existing = await _categoryRepository.GetByIdAsync(categoryId);

            if (existing == null)
            {
                throw new InvalidOperationException($"Category '{categoryId}' not found.");
            }

            existing.Name = updateCategoryDto.Name;
            existing.Description = updateCategoryDto.Description;

            await _categoryRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(string categoryId)
        {
            var existing = await _categoryRepository.GetByIdAsync(categoryId);

            if (existing == null)
            {
                throw new InvalidOperationException($"Category '{categoryId}' not found.");
            }

            await _categoryRepository.DeleteAsync(categoryId);
        }


    }
}