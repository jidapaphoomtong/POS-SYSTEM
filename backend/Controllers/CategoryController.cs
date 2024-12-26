// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading.Tasks;
// using backend.Models;
// using backend.Services;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;

// namespace backend.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class CategoryController : Controller
//     {
//         private readonly FirestoreDB _firestoreDb;

//         public CategoryController(FirestoreDB firestoreDb)
//         {
//             _firestoreDb = firestoreDb;
//         }

//         [HttpPost("add-category")]
//         public async Task<IActionResult> AddCategory([FromBody] Category category){
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(new
//                 {
//                     Success = false,
//                     Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
//                 });
//             }
//             try
//             {
//                 var categoryCollection = _firestoreDb.Collection("categories");
//                 var newCategory = new
//                 {
//                     category.Name
//                 };
//                 var addedCategory = await categoryCollection.AddAsync(newCategory);
//                 return Ok(new
//                 {
//                     Success = true,
//                     Message = "Category added successfully!",
//                     CategoryId = addedCategory.Id
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new
//                 {
//                     Success = false,
//                     Message = "An error occurred while adding the category. " + ex.Message
//                 });
//             }
//         }

//         [HttpGet("get-category")]
//         public async Task<IActionResult> GetAllCategory()
//         {
//             try
//             {
//                 var categoryCollection = _firestoreDb.Collection("categories");
//                 var snapshot = await categoryCollection.GetSnapshotAsync();

//                 if (!snapshot.Documents.Any())
//                 {
//                     return NotFound(new
//                     {
//                         Success = false,
//                         Message = "No category found in Firestore."
//                     });
//                 }

//                 var category = snapshot.Documents.Select(doc => new
//                 {
//                     Id = doc.Id,
//                     Fields = doc.ToDictionary()
//                 }).ToList();

//                 return Ok(new
//                 {
//                     Success = true,
//                     Category = category
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new
//                 {
//                     Success = false,
//                     Message = $"An error occurred while fetching category: {ex.Message}"
//                 });
//             }
//         }

//         [HttpPut("update/{documentId}")]
//         public async Task<IActionResult> UpdateCategory(string documentId, [FromBody] Category category)
//         {
//             try
//             {
//                 var docRef = _firestoreDb.Collection("categories").Document(documentId);
//                 var snapshot = await docRef.GetSnapshotAsync();

//                 if (!snapshot.Exists)
//                     return NotFound(new
//                     {
//                         Success = false,
//                         Message = "Category not found."
//                     });

//                 var updateData = new Dictionary<string, object>();

//                 if (!string.IsNullOrWhiteSpace(category.Name))
//                     updateData["Name"] = category.Name;

//                 if (updateData.Count == 0)
//                 {
//                     return BadRequest(new
//                     {
//                         Success = false,
//                         Message = "No valid fields specified for update."
//                     });
//                 }

//                 await docRef.UpdateAsync(updateData);

//                 return Ok(new
//                 {
//                     Success = true,
//                     Message = "Category updated successfully."
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new
//                 {
//                     Success = false,
//                     Message = $"Error updating Category: {ex.Message}"
//                 });
//             }
//         }

//         [HttpDelete("delete/{documentId}")]
//         public async Task<IActionResult> DeleteCategory([FromRoute] string documentId)
//         {
//             try
//             {
//                 var docRef = _firestoreDb.Collection("categories").Document(documentId);
//                 var snapshot = await docRef.GetSnapshotAsync();

//                 if (!snapshot.Exists)
//                     return NotFound(new
//                     {
//                         Success = false,
//                         Message = "Category not found."
//                     });

//                 await docRef.DeleteAsync();

//                 return Ok(new
//                 {
//                     Success = true,
//                     Message = "Category deleted successfully."
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new
//                 {
//                     Success = false,
//                     Message = $"Error deleting Category: {ex.Message}"
//                 });
//             }
//         }
//     }
// }