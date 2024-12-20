using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using System.Text.Json;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirestoreController : ControllerBase
    {
        private readonly FirestoreDB _firestoreDb;

        public FirestoreController(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // API สำหรับเพิ่มข้อมูลใน Firestore
        [HttpPost("add")]
        public async Task<IActionResult> AddDocument()
        {
            try
            {
                // อ้างอิงถึงคอลเลกชัน "test_collection" และเพิ่มข้อมูล
                var collectionRef = _firestoreDb.Collection("test_collection");

                var newDocument = await collectionRef.AddAsync(new
                {
                    Name = "Sample Data",
                    CreatedAt = DateTime.UtcNow,
                    Description = "Testing Firestore connection"
                });

                // คืนค่า ID ของ Document ที่สร้าง
                return Ok(new { Success = true, DocumentId = newDocument.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        // API สำหรับดึงข้อมูลจาก Firestore
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                // ดึงเอกสารทั้งหมดในคอลเลกชัน "test_collection"
                var collectionRef = _firestoreDb.Collection("test_collection");
                var snapshot = await collectionRef.GetSnapshotAsync();

                var documents = snapshot.Documents
                    .Select(doc => new { doc.Id, Fields = doc.ToDictionary() })
                    .ToList();

                return Ok(new { Success = true, Documents = documents });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        // API สำหรับอัปเดตเอกสาร
        [HttpPut("update/{documentId}")]
        public async Task<IActionResult> UpdateDocument(string documentId, [FromBody] UpdateModel updates)
        {
            try
            {
                var docRef = _firestoreDb.Collection("test_collection").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                    return NotFound(new { Success = false, Message = "Document not found." });

                var updateData = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(updates.Name))
                    updateData["Name"] = updates.Name;

                if (updates.CreatedAt.HasValue)
                    updateData["CreatedAt"] = updates.CreatedAt;

                if (!string.IsNullOrEmpty(updates.Description))
                    updateData["Description"] = updates.Description;

                await docRef.UpdateAsync(updateData);
                return Ok(new { Success = true, Message = "Document updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        // API สำหรับลบเอกสาร(ยังลบไม่ได้เพราะมีปัญหาเกี่ยวกับสิทธิ์การลบ)
        [HttpDelete("delete/{documentId}")]
        public async Task<IActionResult> DeleteDocument([FromRoute] string documentId)
        {
            try
            {
                // Log เอกสารที่อ้างอิง
                Console.WriteLine($"Trying to delete document: {documentId}");

                var docRef = _firestoreDb.Collection("test_collection").Document(documentId);

                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.Exists)
                {
                    Console.WriteLine($"Document not found: {documentId}");
                    return NotFound(new { Success = false, Message = "Document not found." });
                }

                // ลบเอกสาร
                await docRef.DeleteAsync();
                Console.WriteLine($"Document deleted: {documentId}");
                return Ok(new { Success = true, Message = "Document deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document: {ex.Message}");
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}