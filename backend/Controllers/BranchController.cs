using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : Controller
    {
        private readonly FirestoreDB _firestoreDb;

        public BranchController(FirestoreDB firestoreDB)
        {
            _firestoreDb = firestoreDB;
        }

        // เพิ่มข้อมูลใหม่ใน Collection "branch"
        [HttpPost("add-branch")]
        public async Task<IActionResult> AddBranch([FromBody] Branch branch, IFormFile iconFile)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(branch.Name))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Branch Name is required and must be valid."
                });
            }

            try
            {
                var branchCollection = _firestoreDb.Collection("branch");
                string newId = await GenerateNewBranchId();
                var newBranch = new
                {
                    Id = newId,
                    Name = branch.Name,
                    Description = branch.Description,
                    Location = branch.Location,
                    IconUrl = branch.IconUrl
                };
                var addedBranch = await branchCollection.AddAsync(newBranch);
                return Ok(new
                {
                    Success = true,
                    Message = "Branch added successfully!",
                    BranchId = newId,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while adding the branch. " + ex.Message
                });
            }
        }

        private async Task<string> GenerateNewBranchId()
        {
            var settingsCollection = _firestoreDb.Collection("settings");
            var idDocRef = settingsCollection.Document("branchIdTracking");
            var idSnapshot = await idDocRef.GetSnapshotAsync();

            int newId = 1; // เริ่มต้นที่ 1

            // หากมีค่า `lastBranchId` ใน Firestore จะใช้ค่านั้นเพิ่มหนึ่ง
            if (idSnapshot.Exists)
            {
                var currentId = idSnapshot.GetValue<int>("lastBranchId");
                newId = currentId + 1;
            }

            // อัปเดตค่า `lastBranchId` กลับไปยัง Firestore
            await idDocRef.SetAsync(new { lastBranchId = newId });

            // คืนค่าเลข ID ในรูปแบบ 3 หลัก เช่น 001, 002, ...
            return newId.ToString("D3");
        }

        // ดึงข้อมูลทั้งหมดใน Collection "branch"
        [HttpGet("get-branch")]
        public async Task<IActionResult> GetAllBranch()
        {
            try
            {
                var branchCollection = _firestoreDb.Collection("branch");
                var snapshot = await branchCollection.GetSnapshotAsync();

                if (!snapshot.Documents.Any())
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "No branch found in Firestore."
                    });
                }

                var branchList = snapshot.Documents.Select(doc => new
                {
                    Id = doc.Id,
                    Fields = doc.ToDictionary()
                }).ToList();

                return Ok(new
                {
                    Success = true,
                    Branches = branchList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while fetching branch: {ex.Message}"
                });
            }
        }

        // อัปเดตข้อมูลใน Collection "branch"
        [HttpPut("update/{documentId}")]
        public async Task<IActionResult> UpdateBranch(string documentId, [FromBody] Branch branch)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Document ID is required for updating."
                });
            }
            try
            {
                var docRef = _firestoreDb.Collection("branch").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Branch not found."
                    });
                }

                var updateData = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(branch.Name))
                    updateData["Name"] = branch.Name;

                if (!string.IsNullOrWhiteSpace(branch.Description))
                    updateData["Description"] = branch.Description;

                if (!string.IsNullOrWhiteSpace(branch.Location))
                    updateData["Location"] = branch.Location;

                // ถ้าไม่มีข้อมูลที่ต้องการอัปเดตเลย
                if (updateData.Count == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "No valid fields specified for update."
                    });
                }

                await docRef.UpdateAsync(updateData);

                return Ok(new
                {
                    Success = true,
                    Message = "Branch updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while updating branch: {ex.Message}"
                });
            }
        }

        // ลบข้อมูลใน Collection "branch"
        [HttpDelete("delete/{documentId}")]
        public async Task<IActionResult> DeleteBranch([FromRoute] string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Document ID is required for deleting."
                });
            }

            try
            {
                var docRef = _firestoreDb.Collection("branch").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Branch not found."
                    });
                }

                await docRef.DeleteAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Branch deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while deleting branch: {ex.Message}"
                });
            }
        }

        //ลบข้อมูลทั้งหมด และรีเซ็ท id !becarefull!
        [HttpDelete("reset-branches")]
        public async Task<IActionResult> ResetBranches([FromQuery] bool confirm = false)
        {
            if (!confirm)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Confirmation is required to reset branches!"
                });
            }
            try
            {
                // ลบข้อมูลทั้งหมดใน Collection "branch"
                var branchCollection = _firestoreDb.Collection("branch");
                var snapshot = await branchCollection.GetSnapshotAsync();
                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.DeleteAsync();
                }

                // รีเซ็ตค่า lastBranchId ใน Collection "settings"
                var settingsCollection = _firestoreDb.Collection("settings");
                var idDocRef = settingsCollection.Document("branchIdTracking");

                await idDocRef.SetAsync(new { lastBranchId = 0 }); // รีค่าเป็น 0

                return Ok(new
                {
                    Success = true,
                    Message = "All branches deleted and ID reset successfully!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while resetting branches: " + ex.Message
                });
            }
        }
    }
}