
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _productService.GetAllAsync(pageNumber, pageSize);
            return Ok(products);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetById(string productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] AddProductDto addProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid product data.");
            }
            var product = await _productService.AddAsync(addProductDto);
            return Created("Created successfully", product);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateAsync(string productId, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            try
            {
                var product = await _productService.UpdateAsync(productId, updateProductDto);
                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return NotFound("An error occurred while updating the product.");
            }
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete(string productId)
        {
            try
            {
                await _productService.DeleteAsync(productId);
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
                return NotFound("An error occurred while deleting the product.");
            }
        }
    }
}