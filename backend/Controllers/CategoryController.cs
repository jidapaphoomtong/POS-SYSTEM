using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.CategoryService;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

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

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("branches/{branchId}/add-category")]
        public async Task<ActionResult<ServiceResponse<string>>> AddCategory(string branchId, Category category)
        {
            if (string.IsNullOrWhiteSpace(branchId) || category == null)
            {
                return BadRequest("branchId and category must not be null.");
            }

            var response = await _categoryService.AddCategory(branchId, category);
            if (!response.Success) return BadRequest(response.Message);

            return CreatedAtAction(nameof(GetCategoryById), new { branchId = branchId, categoryId = response.Data }, response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("branches/{branchId}/getCategories")]
        public async Task<ActionResult<ServiceResponse<List<Category>>>> GetAllCategories(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return BadRequest("branchId must not be null.");
            }

            var response = await _categoryService.GetAllCategories(branchId);
            return response.Success ? Ok(response) : NotFound(response.Message);
        }

        // [HttpGet("branches/{branchId}/getCategoryById/{categoryId}")]
        // public async Task<ActionResult<ServiceResponse<Category>>> GetCategoryById(string branchId, string categoryId)
        // {
        //     // ตรวจสอบว่าพารามิเตอร์ไม่เป็น null หรือว่าง
        //     if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(categoryId))
        //     {
        //         return BadRequest("branchId and categoryId must not be null or empty.");
        //     }

        //     // ดึงข้อมูลจาก Service
        //     var response = await _categoryService.GetCategoryById(branchId, categoryId);

        //     // ตรวจสอบผลลัพธ์
        //     if (!response.Success)
        //     {
        //         return NotFound(response.Message);
        //     }

        //     return Ok(response);
        // }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("branches/{branchId}/getCategoryById/{categoryId}")]
        public async Task<ActionResult<ServiceResponse<Category>>> GetCategoryById(string branchId, string categoryId)
        {
            var response = await _categoryService.GetCategoryById(branchId, categoryId);
            
            if (response.Success)
            {
                Console.WriteLine("Category found: " + JsonConvert.SerializeObject(response.Data));
                return Ok(response);
            }
            else
            {
                Console.WriteLine("Error: " + response.Message);
                return NotFound(response.Message);
            }
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPut("branches/{branchId}/update/{categoryId}")]
        public async Task<ActionResult<ServiceResponse<string>>> UpdateCategory(string branchId, string categoryId, Category category)
        {
            if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(categoryId) || category == null)
            {
                return BadRequest("branchId, categoryId and category must not be null.");
            }

            category.Id = categoryId; // Ensure category ID is set correctly
            var response = await _categoryService.UpdateCategory(branchId, category);
            if (!response.Success) return BadRequest(response.Message);
            return Ok(response);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("branches/{branchId}/delete/{categoryId}")]
        public async Task<ActionResult<ServiceResponse<string>>> DeleteCategory(string branchId, string categoryId)
        {
            if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(categoryId))
            {
                return BadRequest("branchId and categoryId must not be null.");
            }

            var response = await _categoryService.DeleteCategory(branchId, categoryId);
            if (!response.Success) return NotFound(response.Message);
            return Ok(response);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("branches/{branchId}/delete-all")]
        public async Task<ActionResult<ServiceResponse<string>>> DeleteAllCategories(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return BadRequest("branchId must not be null.");
            }

            var response = await _categoryService.DeleteAllCategories(branchId);
            if (!response.Success) return NotFound(response.Message);
            return Ok(response);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("branches/{branchId}/reset-category-id")]
        public async Task<ActionResult<ServiceResponse<string>>> ResetCategoryId(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return BadRequest("branchId must not be null.");
            }

            var response = await _categoryService.ResetCategoryId(branchId);
            if (!response.Success) return BadRequest(response.Message);
            return Ok(response);
        }
    }
}