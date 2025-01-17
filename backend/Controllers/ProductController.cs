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
            var response = await _productService.AddProduct(branchId, product);
            if (response.Success) return Ok(response);
            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("products/{branchId}/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(string branchId, string categoryId)
        {
            var response = await _productService.GetProductsByCategory(branchId, categoryId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("branches/{branchId}/products")]
        public async Task<IActionResult> GetProducts(string branchId)
        {
            var result = await _productService.GetProducts(branchId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> GetProductById(string branchId,  string productId)
        {
            var result = await _productService.GetProductById(branchId, productId);
            return result.Success ? Ok(result.Data) : NotFound(result.Message);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPut("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> UpdateProduct(string branchId, string productId, [FromBody] Products updatedProduct)
        {
            var response = await _productService.UpdateProduct(branchId, productId, updatedProduct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> DeleteProduct(string branchId, string productId)
        {
            var response = await _productService.DeleteProduct(branchId, productId);
            return response.Success ? Ok(response) : BadRequest(response);
        }
        
        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("{branchId}/products")]
        public async Task<IActionResult> DeleteAllProducts(string branchId)
        {
            var response = await _productService.DeleteAllProducts(branchId);
            if (response.Success) return Ok(response);
            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager")]
        // Reset product ID sequence for a specific branch
        [HttpPost("branches/{branchId}/reset-product-id")]
        public async Task<IActionResult> ResetProductId(string branchId)
        {
            var response = await _productService.ResetproductId(branchId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        // [CustomAuthorizeRole("Admin, Manager, Employee")]
        // [HttpGet("branches/{branchId}/products")]
        // public async Task<IActionResult> GetProducts(string branchId)
        // {
        //     try
        //     {
        //         var result = await _productService.GetProducts(branchId);
        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // [CustomAuthorizeRole("Admin, Manager, Employee")]
        // [HttpGet("{branchId}/products/{productId}")]
        // public async Task<IActionResult> GetProductById(string branchId, string productId)
        // {
        //     var result = await _productService.GetProductById(branchId, productId);
        //     if (result.Success)
        //     {
        //         return Ok(result.Data);  // ส่งข้อมูลสินค้ากลับ
        //     }
        //     return NotFound(result.Message);
        // }

        // [CustomAuthorizeRole("Admin, Manager")]
        // [HttpPut("branches/{branchId}/products/{productId}")]
        // public async Task<IActionResult> UpdateProduct(string branchId, string productId, [FromBody] Products updatedProduct)
        // {
        //     try
        //     {
        //         await _productService.UpdateProduct(branchId, productId, updatedProduct);
        //         return Ok(new { message = "Product updated successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // [CustomAuthorizeRole("Admin, Manager")]
        // [HttpDelete("branches/{branchId}/products/{productId}")]
        // public async Task<IActionResult> DeleteProduct(string branchId, string productId)
        // {
        //     try
        //     {
        //         await _productService.DeleteProduct(branchId, productId);
        //         return Ok(new { message = "Product deleted successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }
    }
}