using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace backend.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly FirestoreDB _firestoreDb;

        public CategoryService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<string> GetNextId(string sequenceName)
        {
            try
            {
                var sequenceDoc = _firestoreDb.Collection(FirestoreCollections.Config).Document(sequenceName);
                var snapshot = await sequenceDoc.GetSnapshotAsync();

                int counter = 1;

                if (snapshot.Exists && snapshot.TryGetValue<int>("counter", out var currentCounter))
                {
                    counter = currentCounter;
                }

                // Increment ลำดับ
                await sequenceDoc.SetAsync(new { counter = counter + 1 });

                // คืนค่า ID ในรูปแบบ "001", "002", "003"
                return counter.ToString("D3");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate ID for {sequenceName}: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> AddCategory(string branchId, Category category)
        {
            if (string.IsNullOrWhiteSpace(branchId) || category == null)
            {
                return ServiceResponse<string>.CreateFailure("branchId or category wrong!");
            }

            try
            {
                string categoryId = await GetNextId($"category-sequence-{branchId}"); // ลำดับเฉพาะต่อ Branch

                DocumentReference categoryDoc = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Categories)
                    .Document(categoryId);

                category.Id = categoryId;
                await categoryDoc.SetAsync(category);
                return ServiceResponse<string>.CreateSuccess(category.Id, "Category added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add category: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Category>>> GetAllCategories(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return ServiceResponse<List<Category>>.CreateFailure("branchId NOT NULL");
            }

            try
            {
                var categories = new List<Category>();
                var snapshot = await _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Categories)
                    .GetSnapshotAsync();

                foreach (var categoryDoc in snapshot.Documents)
                {
                    var category = categoryDoc.ConvertTo<Category>();
                    // Console.WriteLine(JsonConvert.SerializeObject(category));
                    categories.Add(category);
                }

                return ServiceResponse<List<Category>>.CreateSuccess(categories, "Categories fetched successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Category>>.CreateFailure($"Failed to fetch categories: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Category>> GetCategoryById(string branchId, string categoryId)
        {
            // ตรวจสอบว่า branchId และ categoryId ไม่เป็น null หรือว่าง
            if (string.IsNullOrWhiteSpace(branchId))
            {
                throw new ArgumentException("Branch ID cannot be null or empty.", nameof(branchId));
            }
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                throw new ArgumentException("Category ID cannot be null or empty.", nameof(categoryId));
            }

            try
            {
                // สร้าง DocumentReference สำหรับ branch และ category
                var branchDoc = _firestoreDb.Collection(FirestoreCollections.Branches).Document(branchId);
                var categoryDoc = branchDoc.Collection(FirestoreCollections.Categories).Document(categoryId);

                // ดึงเอกสารหมวดหมู่จาก Firestore
                DocumentSnapshot snapshot = await categoryDoc.GetSnapshotAsync();

                // ตรวจสอบว่าเอกสารหมวดหมู่มีอยู่
                if (!snapshot.Exists)
                {
                    Console.WriteLine($"Category with ID {categoryId} not found in branch {branchId}.");
                    return ServiceResponse<Category>.CreateFailure("Category not found.");
                }

                // แปลงเอกสารเป็น Category
                var category = snapshot.ConvertTo<Category>();
                Console.WriteLine("Category Data: " + JsonConvert.SerializeObject(category));

                // ส่งกลับข้อมูลหมวดหมู่ที่ถูกต้อง
                return ServiceResponse<Category>.CreateSuccess(category, "Category fetched successfully!");
            }
            catch (Exception ex)
            {
                // จัดการข้อผิดพลาดและส่งข้อความที่เหมาะสม
                Console.WriteLine($"Error fetching category: {ex}");
                return ServiceResponse<Category>.CreateFailure($"Failed to fetch category: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> UpdateCategory(string branchId, Category category)
        {
            try
            {
                var categoryDoc = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Categories)
                    .Document(category.Id);

                await categoryDoc.SetAsync(category, SetOptions.MergeAll);
                return ServiceResponse<string>.CreateSuccess(category.Id, "Category updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update category: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> DeleteCategory(string branchId, string categoryId)
        {
            try
            {
                var categoryDoc = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Categories)
                    .Document(categoryId);

                await categoryDoc.DeleteAsync();
                return ServiceResponse<string>.CreateSuccess(categoryId, "Category deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete category: {ex.Message}");
            }
        }
        public async Task<ServiceResponse<string>> DeleteAllCategories(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return ServiceResponse<string>.CreateFailure("branchId NOT NULL");
            }

            try
            {
                var categoriesCollection = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Categories);

                var snapshots = await categoriesCollection.GetSnapshotAsync();
                var deleteTasks = snapshots.Documents.Select(doc => doc.Reference.DeleteAsync());

                await Task.WhenAll(deleteTasks);
                return ServiceResponse<string>.CreateSuccess(null, "All category deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete category: {ex.Message}");
            }
        }

        // ฟังก์ชันรีเซ็ต ID หมวดหมู่
        public async Task<ServiceResponse<string>> ResetCategoryId(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return ServiceResponse<string>.CreateFailure("branchId NOT NULL");
            }

            try
            {
                var sequenceDoc = _firestoreDb.Collection(FirestoreCollections.Config).Document($"category-sequence-{branchId}");
                await sequenceDoc.SetAsync(new { counter = 1 }); // รีเซ็ตค่าเป็น 1

                return ServiceResponse<string>.CreateSuccess("Category ID reset already!", "Reset done");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Can not reset category id : {ex.Message}");
            }
        }
    }
}