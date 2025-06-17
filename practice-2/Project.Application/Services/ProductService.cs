
using Application.Common.Models;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<PaginatedResult<GetProductDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _productRepository.GetAllAsync().Include(p => p.Category);
            var totalCount = await query.CountAsync();

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(p => new GetProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Category = new GetCategoryDto
                {
                    CategoryId = p.Category.CategoryId,
                    Name = p.Category.Name,
                    Description = p.Category.Description

                }

            })
            .ToListAsync();

            return new PaginatedResult<GetProductDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount

            };
        }

        public async Task<GetProductDto> GetByIdAsync(string productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                return null;

            return new GetProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description
            };
        }

        public async Task<GetProductDto> AddAsync(AddProductDto addProductDto)
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid().ToString(),
                Name = addProductDto.Name,
                Description = addProductDto.Description,
                CategoryId = addProductDto.CategoryId
            };

            await _productRepository.AddAsync(product);

            return new GetProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description
            };
        }

        public async Task<GetProductDto> UpdateAsync(string productId, UpdateProductDto updateProductDto)
        {
            var existing = await _productRepository.GetByIdAsync(productId);

            if (existing == null)
            {
                throw new InvalidOperationException($"Product '{productId}' not found.");
            }

            existing.Name = updateProductDto.Name;
            existing.Description = updateProductDto.Description;

            await _productRepository.UpdateAsync(existing);

            return new GetProductDto
            {
                ProductId = existing.ProductId,
                Name = existing.Name,
                Description = existing.Description
            };
        }

        public async Task DeleteAsync(string productId)
        {
            var existing = await _productRepository.GetByIdAsync(productId);

            if (existing == null)
            {
                throw new InvalidOperationException($"Product '{productId}' not found.");
            }

            await _productRepository.DeleteAsync(productId);
        }


    }
}