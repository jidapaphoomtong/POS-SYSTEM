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

        // Endpoint สำหรับอัปเดตสถานะของผลิตภัณฑ์
        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPut("branches/{branchId}/products/{productId}/status")]
        public async Task<IActionResult> UpdateStatus(string branchId, string productId, [FromBody] StatusRequest request)
        {
            // อัปเดตสถานะและเช็คว่าประสบความสำเร็จหรือไม่
            var result = await _productService.UpdateProductStatusAsync(branchId, productId, request.status);
            
            if (!result)
            {
                return NotFound(new { message = "Product not found." }); // คืนค่าถ้าไม่พบผลิตภัณฑ์
            }
            
            return Ok(new { message = "Product status updated successfully." }); //คืนค่าความสำเร็จ
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpPut("update-stock/{branchId}/{productId}")]
        public async Task<ActionResult<ServiceResponse<string>>> UpdateStock(string branchId, string productId, [FromBody] Products product)
        {
            if (product == null || product.quantity <= 0)
            {
                return BadRequest("Invalid product data. Quantity must be greater than zero.");
            }

            // เช็คสถานะก่อนการอัปเดต
            var currentProductResponse = await _productService.GetProductById(branchId, productId);
            if (!currentProductResponse.Success || currentProductResponse.Data.status == "inactive")
            {
                return BadRequest("Cannot order the product. The product is out of stock or inactive.");
            }

            var response = await _productService.UpdateStock(branchId, productId, product);
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