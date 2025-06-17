using System;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _svc;

        public CategoriesController(ICategoryService svc)
        {
            _svc = svc;
        }

        // GET: api/products?page=1&size=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedResult = await _svc.GetAllAsync(pageNumber, pageSize);
            return Ok(pagedResult);
        }

        // GET: api/products/{id}
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetById(string categoryId)
        {
            var dto = await _svc.GetByIdAsync(categoryId);
            if (dto == null)
                return NotFound($"Category with id '{categoryId}' not found.");

            return Ok(dto);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddCategoryDto addDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid category data.");

            var createdDto = await _svc.AddAsync(addDto);
            // Assuming AddAsync returns the created GetProductDto (with its new Id)
            return Created("Product created successfully.", createdDto);
        }

        // PUT: api/products/{id}
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> Update(string categoryId, [FromBody] UpdateCategoryDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid product data.");

            try
            {
                await _svc.UpdateAsync(categoryId, updateDto);
                return Ok(updateDto);
            }
            catch (InvalidOperationException ex)
            {
                // thrown when the category wasn't found
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                // catch-all for unexpected errors
                return NotFound("An error occurred while updating the category.");
            }

        }

        // DELETE: api/products/{id}
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> Delete(string categoryId)
        {
            try
            {
                await _svc.DeleteAsync(categoryId);
                return Ok("Deleted Successfully");
            }
            catch (InvalidOperationException ex)
            {
                // thrown when the category wasn't found
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                // catch-all for unexpected errors
                return NotFound("An error occurred while updating the category.");
            }
        }
    }
}