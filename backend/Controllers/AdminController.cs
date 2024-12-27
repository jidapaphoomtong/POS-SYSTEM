using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using backend.Services.AdminService;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // เพิ่ม Branch เข้า Collection "branches"
        [HttpPost("add-branch")]
        public async Task<IActionResult> AddBranch([FromBody] Branch branch)
        {
            try
            {
                var result = await _adminService.AddBranch(branch);
                return Ok(new { message = "Branch added successfully", documentId = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // เพิ่ม Employee เข้า Subcollection "employees" ใน Branch (Dynamic Branch)
        [HttpPost("add-employee/{branchId}")]
        public async Task<IActionResult> AddEmployee(string branchId, [FromBody] Employee employee)
        {
            try
            {
                var result = await _adminService.AddEmployee(branchId, employee);
                return Ok(new { message = "Employee added successfully", documentId = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // กำหนด Product เข้า Subcollection "products"
        [HttpPost("add-product/{branchId}")]
        public async Task<IActionResult> AddProduct(string branchId, [FromBody] Products product)
        {
            try
            {
                var result = await _adminService.AddProduct(branchId, product);
                return Ok(new { message = "Product added successfully", documentId = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ดึงข้อมูล Branches
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var result = await _adminService.GetBranches();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // อัปเดต Branch
        [HttpPut("branches/{branchId}")]
        public async Task<IActionResult> UpdateBranch(string branchId, [FromBody] Dictionary<string, object> updatedData)
        {
            try
            {
                await _adminService.UpdateBranch(branchId, updatedData);
                return Ok(new { message = "Branch updated successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ลบ Branch
        [HttpDelete("branches/{branchId}")]
        public async Task<IActionResult> DeleteBranch(string branchId)
        {
            try
            {
                await _adminService.DeleteBranch(branchId);
                return Ok(new { message = "Branch deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // พนักงาน
        [HttpGet("branches/{branchId}/employees")]
        public async Task<IActionResult> GetEmployees(string branchId)
        {
            try
            {
                var result = await _adminService.GetEmployees(branchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("branches/{branchId}/employees/{employeeId}")]
        public async Task<IActionResult> UpdateEmployee(string branchId, string employeeId, [FromBody] Employee updatedEmployee)
        {
            try
            {
                await _adminService.UpdateEmployee(branchId, employeeId, updatedEmployee);
                return Ok(new { message = "Employee updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("branches/{branchId}/employees/{employeeId}")]
        public async Task<IActionResult> DeleteEmployee(string branchId, string employeeId)
        {
            try
            {
                await _adminService.DeleteEmployee(branchId, employeeId);
                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // สินค้า
        [HttpGet("branches/{branchId}/products")]
        public async Task<IActionResult> GetProducts(string branchId)
        {
            try
            {
                var result = await _adminService.GetProducts(branchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> UpdateProduct(string branchId, string productId, [FromBody] Products updatedProduct)
        {
            try
            {
                await _adminService.UpdateProduct(branchId, productId, updatedProduct);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> DeleteProduct(string branchId, string productId)
        {
            try
            {
                await _adminService.DeleteProduct(branchId, productId);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}