using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DepartmentController : Controller
    {
        private readonly FirestoreDB _firestoreDb;
        public DepartmentController(FirestoreDB firestoreDB)
        {
            _firestoreDb = firestoreDB;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddDepartment([FromBody] Department department)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            try
            {
                var departmentCollection = _firestoreDb.Collection("departments");
                var newDepartment = new
                {
                    department.Name,
                    department.Description,
                    department.Location
                };
                var addedDepartment = await departmentCollection.AddAsync(newDepartment);
                return Ok(new
                {
                    Success = true,
                    Message = "Department added successfully!",
                    DepartmentId = addedDepartment.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while adding the department. " + ex.Message
                });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departmentCollection = _firestoreDb.Collection("departments");
                var snapshot = await departmentCollection.GetSnapshotAsync();

                if (!snapshot.Documents.Any())
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "No departments found in Firestore."
                    });
                }

                var departments = snapshot.Documents.Select(doc => new
                {
                    Id = doc.Id,
                    Fields = doc.ToDictionary()
                }).ToList();

                return Ok(new
                {
                    Success = true,
                    Departments = departments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while fetching departments: {ex.Message}"
                });
            }
        }

        [HttpPut("update/{documentId}")]
        public async Task<IActionResult> UpdateDepartment(string documentId, [FromBody] Department department)
        {
            try
            {
                var docRef = _firestoreDb.Collection("departments").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Department not found."
                    });

                var updateData = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(department.Name))
                    updateData["Name"] = department.Name;

                if (!string.IsNullOrWhiteSpace(department.Description))
                    updateData["Description"] = department.Description;

                if (!string.IsNullOrWhiteSpace(department.Location))
                    updateData["Location"] = department.Location;

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
                    Message = "Department updated successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error updating department: {ex.Message}"
                });
            }
        }

        [HttpDelete("delete/{documentId}")]
        public async Task<IActionResult> DeleteDepartment([FromRoute] string documentId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("departments").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Department not found."
                    });

                await docRef.DeleteAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Department deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error deleting department: {ex.Message}"
                });
            }
        }
    }
}