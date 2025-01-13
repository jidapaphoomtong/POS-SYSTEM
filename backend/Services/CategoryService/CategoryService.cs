using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;

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
                var sequenceDoc = _firestoreDb.Collection("config").Document(sequenceName);
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
            try
            {
                string categoryId = await GetNextId($"category-sequence-{branchId}"); // ลำดับเฉพาะต่อ Branch

                DocumentReference categoryDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("categories")
                    .Document();

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
            try
            {
                var categories = new List<Category>();
                var snapshot = await _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("categories")
                    .GetSnapshotAsync();

                foreach (var categoryDoc in snapshot.Documents)
                {
                    var category = categoryDoc.ConvertTo<Category>();
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
            try
            {
                var categoryDoc = await _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("categories")
                    .Document(categoryId)
                    .GetSnapshotAsync();

                if (!categoryDoc.Exists)
                {
                    return ServiceResponse<Category>.CreateFailure("Category not found.");
                }

                var category = categoryDoc.ConvertTo<Category>();
                return ServiceResponse<Category>.CreateSuccess(category, "Category fetched successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Category>.CreateFailure($"Failed to fetch category: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> UpdateCategory(string branchId, Category category)
        {
            try
            {
                var categoryDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("categories")
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
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("categories")
                    .Document(categoryId);

                await categoryDoc.DeleteAsync();
                return ServiceResponse<string>.CreateSuccess(categoryId, "Category deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete category: {ex.Message}");
            }
        }
    }
}