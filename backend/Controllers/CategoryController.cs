using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.CategoryService;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [LogAction]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("branches/{branchId}/add-category")]
        public async Task<ActionResult<ServiceResponse<string>>> AddCategory(string branchId, Category category)
        {
            var response = await _categoryService.AddCategory(branchId, category);
            if (!response.Success) return BadRequest(response.Message);
            return CreatedAtAction(nameof(GetCategoryById), new { branchId = branchId, categoryId = response.Data }, response);
        }

        [HttpGet("branches/{branchId}/getCategory")]
        public async Task<ActionResult<ServiceResponse<List<Category>>>> GetAllCategories(string branchId)
        {
            var response = await _categoryService.GetAllCategories(branchId);
            return response.Success ? Ok(response) : NotFound(response.Message);
        }

        [HttpGet("branches/{branchId}/getCategoryById")]
        public async Task<ActionResult<ServiceResponse<Category>>> GetCategoryById(string branchId, string categoryId)
        {
            var response = await _categoryService.GetCategoryById(branchId, categoryId);
            if (!response.Success) return NotFound(response.Message);
            return Ok(response);
        }

        [HttpPut("branches/{branchId}/update/{categoryId}")]
        public async Task<ActionResult<ServiceResponse<string>>> UpdateCategory(string branchId, Category category)
        {
            var response = await _categoryService.UpdateCategory(branchId, category);
            if (!response.Success) return BadRequest(response.Message);
            return NoContent();
        }

        [HttpDelete("branches/{branchId}/delate/{categoryId}")]
        public async Task<ActionResult<ServiceResponse<string>>> DeleteCategory(string branchId, string categoryId)
        {
            var response = await _categoryService.DeleteCategory(branchId, categoryId);
            if (!response.Success) return NotFound(response.Message);
            return NoContent();
        }
    }
}