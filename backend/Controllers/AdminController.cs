using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using backend.Services.AdminService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        // private readonly IAdminService _adminService;

        // public AdminController(IAdminService adminService)
        // {
        //     _adminService = adminService;
        // }

        // // เพิ่ม Branch เข้า Collection "branches"
        // [HttpPost("add-branch")]
        // public async Task<IActionResult> AddBranch([FromBody] Branch branch)
        // {
        //     var response = await _adminService.AddBranch(branch);
        //     if (response.Success) return Ok(response);
        //     return BadRequest(response);
        // }

        // [HttpPost("add-employee/{branchId}")]
        // public async Task<IActionResult> AddEmployee(string branchId, [FromBody] Employee employee)
        // {
        //     var response = await _adminService.AddEmployee(branchId, employee);
        //     if (response.Success) return Ok(response);
        //     return BadRequest(response);
        // }

        // [HttpPost("add-product/{branchId}")]
        // public async Task<IActionResult> AddProduct(string branchId, [FromBody] Products product)
        // {
        //     var response = await _adminService.AddProduct(branchId, product);
        //     if (response.Success) return Ok(response);
        //     return BadRequest(response);
        // }

        // [HttpGet("branches")]
        // public async Task<IActionResult> GetBranches()
        // {
        //     var response = await _adminService.GetBranches();
        //     if (response.Success) return Ok(response.Data);
        //     return BadRequest(response);
        // }

        // // อัปเดต Branch
        // [HttpPut("branches/{branchId}")]
        // public async Task<IActionResult> UpdateBranch(string branchId, [FromBody] Dictionary<string, object> updatedData)
        // {
        //     try
        //     {
        //         await _adminService.UpdateBranch(branchId, updatedData);
        //         return Ok(new { message = "Branch updated successfully!" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // // ลบ Branch
        // [HttpDelete("branches/{branchId}")]
        // public async Task<IActionResult> DeleteBranch(string branchId)
        // {
        //     try
        //     {
        //         await _adminService.DeleteBranch(branchId);
        //         return Ok(new { message = "Branch deleted successfully!" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // // พนักงาน
        // [HttpGet("branches/{branchId}/employees")]
        // public async Task<IActionResult> GetEmployees(string branchId)
        // {
        //     try
        //     {
        //         var result = await _adminService.GetEmployees(branchId);
        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // [HttpPut("branches/{branchId}/employees/{employeeId}")]
        // public async Task<IActionResult> UpdateEmployee(string branchId, string employeeId, [FromBody] Employee updatedEmployee)
        // {
        //     try
        //     {
        //         await _adminService.UpdateEmployee(branchId, employeeId, updatedEmployee);
        //         return Ok(new { message = "Employee updated successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // [HttpDelete("branches/{branchId}/employees/{employeeId}")]
        // public async Task<IActionResult> DeleteEmployee(string branchId, string employeeId)
        // {
        //     try
        //     {
        //         await _adminService.DeleteEmployee(branchId, employeeId);
        //         return Ok(new { message = "Employee deleted successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // // สินค้า
        // [HttpGet("branches/{branchId}/products")]
        // public async Task<IActionResult> GetProducts(string branchId)
        // {
        //     try
        //     {
        //         var result = await _adminService.GetProducts(branchId);
        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // [HttpPut("branches/{branchId}/products/{productId}")]
        // public async Task<IActionResult> UpdateProduct(string branchId, string productId, [FromBody] Products updatedProduct)
        // {
        //     try
        //     {
        //         await _adminService.UpdateProduct(branchId, productId, updatedProduct);
        //         return Ok(new { message = "Product updated successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // [HttpDelete("branches/{branchId}/products/{productId}")]
        // public async Task<IActionResult> DeleteProduct(string branchId, string productId)
        // {
        //     try
        //     {
        //         await _adminService.DeleteProduct(branchId, productId);
        //         return Ok(new { message = "Product deleted successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        [Authorize(Roles = "admin")]
        [HttpGet("view-bran")]
        public IActionResult ViewBranch()
        {
            return Ok(new List<string> { "Branch A", "Branch B", "Branch C" });
        }

        [Authorize(Roles = "manager")]
        [HttpGet("view-employee")]
        public IActionResult ViewEmployee()
        {
            return Ok(new List<string> { "Employee A", "Employee B", "Employee C" });
        }

        [Authorize(Roles = "employee, manager")]
        [HttpGet("view-products")]
        public IActionResult ViewProducts()
        {
            return Ok(new List<string> { "Product A", "Product B", "Product C" });
        }
    }
}