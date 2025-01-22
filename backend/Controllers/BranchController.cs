using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.BranchService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [DisableCors]
    [LogAction]
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        // เพิ่ม Branch เข้า Collection "branches"
        [CustomAuthorizeRole("Admin")] // ระบุว่าต้องเป็น Role "Admin" เท่านั้นถึงเข้าถึงได้
        [HttpPost("add-branch")]
        public async Task<IActionResult> AddBranch([FromBody] Branch branch)
        {
            // ดึงข้อมูล User จาก Claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;                   // Role จาก Jwt Token
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;                   // ชื่อ user จาก Jwt Token

            // เรียกใช้ Service เพิ่มสาขา (Branch)
            var response = await _branchService.AddBranch(branch);

            if (response.Success)
                return Ok(new { Success = true, Message = response.Message, BranchId = response.Data });

            return BadRequest(new { Success = false, Message = response.Message });
        }

        [CustomAuthorizeRole("Admin")]
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            // ตรวจสอบว่าผู้ใช้ Authenticated หรือไม่
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Success = false, Message = "Unauthorized. Please login." });
            }

            // ดึงข้อมูล User และ Role
            var user = User.Identity?.Name; // ดึงชื่อผู้ใช้
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value); // ดึง Role

            // Log ข้อมูล User และ Role
            Console.WriteLine($"User: {user}, Roles: {string.Join(", ", roles)}");

            // // ตรวจสอบ Role ว่ามีสิทธิ์เข้าถึงหรือไม่
            // if (!roles.Contains("Admin"))
            // {
            //     return Forbid(new { Success = false, Message = "Access Denied: Admin role required." });
            // }

            // เรียก Service เพื่อดึงข้อมูล Branches
            var response = await _branchService.GetBranches();

            // ตอบกลับผลลัพธ์
            if (response.Success)
            {
                Console.WriteLine("Branches fetched successfully.");
                return Ok(new { success = true, data = response.Data });
            }
            else
            {
                Console.WriteLine($"Error fetching branches: {response.Message}");
                return BadRequest(new { success = false, message = response.Message });
            }
        }

        // อัปเดต Branch
        [CustomAuthorizeRole("Admin")]
        [HttpPut("branches/{branchId}")]
        public async Task<IActionResult> UpdateBranch(string branchId, [FromBody] Branch updatedBranch)
        {
            try
            {
                // ตรวจสอบข้อมูล branch
                if (string.IsNullOrWhiteSpace(updatedBranch.Name) || 
                    string.IsNullOrWhiteSpace(updatedBranch.Location) || 
                    string.IsNullOrWhiteSpace(updatedBranch.IconUrl))
                {
                    return BadRequest(new { Success = false, Message = "Invalid branch data." });
                }

                // ดึงข้อมูล branch เดิมจาก Firestore
                var existingBranchResponse = await _branchService.GetBranchById(branchId);
                if (!existingBranchResponse.Success)
                {
                    return NotFound(new { Success = false, Message = "Branch not found." });
                }

                // สร้าง branch ที่ต้องการอัปเดต
                var branchToUpdate = new Branch
                {
                    Name = string.IsNullOrWhiteSpace(updatedBranch.Name) ? existingBranchResponse.Data.Name : updatedBranch.Name,
                    Location = string.IsNullOrWhiteSpace(updatedBranch.Location) ? existingBranchResponse.Data.Location : updatedBranch.Location,
                    IconUrl = string.IsNullOrWhiteSpace(updatedBranch.IconUrl) ? existingBranchResponse.Data.IconUrl : updatedBranch.IconUrl,
                };

                // อัปเดต branch ใน Firestore
                var updateResponse = await _branchService.UpdateBranch(branchId, branchToUpdate);
                if (!updateResponse.Success)
                {
                    return BadRequest(new { Success = false, Message = "Failed to update branch." });
                }

                return Ok(new { Success = true, Message = "Branch updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("branches/{branchId}/status")]
        public async Task<IActionResult> UpdateStatus(string branchId, [FromBody] StatusRequest request)
        {
            var result = await _branchService.UpdateBranchStatusAsync(branchId, request.status);

            if (!result)
            {
                return NotFound(new { message = "Branch not found." });
            }

            return Ok(new { message = "Branch status updated successfully." });
        }

        [CustomAuthorizeRole("Admin")]
        [HttpGet("branches/{branchId}")]
        public async Task<IActionResult> GetBranchById(string branchId)
        {
            try
            {
                // Log ID ที่เรียก
                Console.WriteLine($"Requesting branch with ID: {branchId}");

                // เรียกใช้ AdminService เพื่อตรวจสอบ branch
                var response = await _branchService.GetBranchById(branchId);

                // ตรวจสอบว่ามีข้อมูลหรือไม่
                if (!response.Success)
                {
                    return NotFound(new { Success = false, Message = response.Message });
                }

                return Ok(new { Success = true, Data = response.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ลบ Branch
        [CustomAuthorizeRole("Admin")]
        [HttpDelete("branches/{branchId}")]
        public async Task<IActionResult> DeleteBranch(string branchId)
        {
            try
            {
                await _branchService.DeleteBranch(branchId);
                return Ok(new { message = "Branch deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // [CustomAuthorizeRole("Admin")]
        // [HttpDelete("branches")]
        // public async Task<IActionResult> DeleteAllBranches()
        // {
        //     var response = await _branchService.DeleteAllBranches();

        //     if (response.Success)
        //     {
        //         return Ok(new { Success = true, Message = response.Message });
        //     }

        //     return BadRequest(new { Success = false, Message = response.Message });
        // }

        // [CustomAuthorizeRole("Admin")]
        // [HttpPost("reset-branch-sequence")]
        // public async Task<IActionResult> ResetBranchIdSequence()
        // {
        //     var response = await _branchService.ResetBranchIdSequence();

        //     if (response.Success)
        //     {
        //         return Ok(new { Success = true, Message = response.Message });
        //     }

        //     return BadRequest(new { Success = false, Message = response.Message });
        // }
    }
}