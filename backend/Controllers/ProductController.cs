using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.ProductService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [DisableCors]
    [LogAction]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("add-product/{branchId}")]
        public async Task<IActionResult> AddProduct(string branchId, [FromBody] Products product)
        {
            // ดึง User จาก Claim
            var userName = User.FindFirst(ClaimTypes.Name)?.Value; // ชื่อ user จาก Jwt Token
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value; // ดึง Role จาก Claim
            
            var response = await _productService.AddProduct(branchId, product);
            if (response.Success) return Ok(response);
            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("branches/{branchId}/products")]
        public async Task<IActionResult> GetProducts(string branchId)
        {
            try
            {
                var result = await _productService.GetProducts(branchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("{branchId}/products/{productId}")]
        public async Task<IActionResult> GetProductById(string branchId, string productId)
        {
            var result = await _productService.GetProductById(branchId, productId);
            if (result.Success)
            {
                return Ok(result.Data);  // ส่งข้อมูลสินค้ากลับ
            }
            return NotFound(result.Message);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPut("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> UpdateProduct(string branchId, string productId, [FromBody] Products updatedProduct)
        {
            try
            {
                await _productService.UpdateProduct(branchId, productId, updatedProduct);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> DeleteProduct(string branchId, string productId)
        {
            try
            {
                await _productService.DeleteProduct(branchId, productId);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("{branchId}/products/{productId}/addstock")]
        public async Task<IActionResult> AddStock(string branchId, string productId, [FromBody] int quantity)
        {
            var result = await _productService.AddStock(branchId, productId, quantity);
            if (result.Success)
            {
                return Ok(result.Message);  // ส่งข้อความสำเร็จ
            }
            return BadRequest(result.Message);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("{branchId}/products/{productId}/reducestock")]
        public async Task<IActionResult> ReduceStock(string branchId, string productId, [FromBody] int quantity)
        {
            var result = await _productService.ReduceStock(branchId, productId, quantity);
            if (result.Success)
            {
                return Ok(result.Message);  // ส่งข้อความสำเร็จ
            }
            return BadRequest(result.Message);
        }
    }
}