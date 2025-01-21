using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly FirestoreDB _firestoreDb;

        public ProductService(FirestoreDB firestoreDb)
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

        // public async Task<ServiceResponse<string>> AddProduct(string branchId, string categoryId, Products product)
        // {
        //     try
        //     {
        //         var categoryDoc = _firestoreDb.Collection("branches")
        //                         .Document(branchId)
        //                         .Collection("categories")
        //                         .Document(categoryId);
        //         var snapshot = await categoryDoc.GetSnapshotAsync();
                
        //         if (!snapshot.Exists)
        //         {
        //             return ServiceResponse<string>.CreateFailure("หมวดหมู่ไม่อยู่ในระบบ.");
        //         }

        //         string productId = await GetNextId($"product-sequence-{branchId}");

        //         DocumentReference productDoc = categoryDoc.Collection("products").Document(productId);
        //         product.Id = productId;
        //         product.categoryId = categoryId; // ตั้งค่า categoryId ที่อ้างอิง
        //         product.branchId = branchId;      // ตั้งค่า branchId

        //         await productDoc.SetAsync(product);
        //         return ServiceResponse<string>.CreateSuccess(productId, "เพิ่มผลิตภัณฑ์สำเร็จ!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return ServiceResponse<string>.CreateFailure($"ไม่สามารถเพิ่มผลิตภัณฑ์ได้: {ex.Message}");
        //     }
        // }

        // public async Task<ServiceResponse<List<Products>>> GetProductsByCategory(string branchId, string categoryId)
        // {
        //     try
        //     {
        //         CollectionReference products = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("categories")
        //             .Document(categoryId)
        //             .Collection("products");

        //         QuerySnapshot querySnapshot = await products.GetSnapshotAsync();
        //         List<Products> productsList = querySnapshot.Documents.Select(doc => doc.ConvertTo<Products>()).ToList();
        //         return ServiceResponse<List<Products>>.CreateSuccess(productsList, "Products fetched successfully!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return ServiceResponse<List<Products>>.CreateFailure($"Failed to fetch products: {ex.Message}");
        //     }
        // }

        // public async Task<ServiceResponse<List<ProductsResponse>>> GetProducts(string branchId, string categoryId)
        // {
        //     try
        //     {
        //         CollectionReference products = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("categories")
        //             .Document(categoryId)
        //             .Collection("products");

        //         QuerySnapshot snapshot = await products.GetSnapshotAsync();
                
        //         var productsList = snapshot.Documents.Select(doc => new ProductsResponse
        //         {
        //             Id = doc.Id,
        //             Data = doc.ToDictionary()
        //         }).ToList();
                
        //         // ดึงข้อมูลหมวดหมู่
        //         var categoryDoc = await _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("categories")
        //             .Document(categoryId)
        //             .GetSnapshotAsync();

        //         var categoryData = categoryDoc.Exists ? categoryDoc.ConvertTo<Category>() : null;

        //         return ServiceResponse<List<ProductsResponse>>.CreateSuccess(productsList, "ดึงข้อมูลผลิตภัณฑ์สำเร็จ!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return ServiceResponse<List<ProductsResponse>>.CreateFailure($"ไม่สามารถดึงข้อมูลผลิตภัณฑ์ได้: {ex.Message}");
        //     }
        // }

        // public async Task<ServiceResponse<Products>> GetProductById(string branchId, string categoryId, string productId)
        // {
        //     try
        //     {
        //         DocumentReference productDoc = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("categories")
        //             .Document(categoryId)
        //             .Collection("products")
        //             .Document(productId);

        //         DocumentSnapshot snapshot = await productDoc.GetSnapshotAsync();

        //         if (!snapshot.Exists)
        //         {
        //             return ServiceResponse<Products>.CreateFailure("Product not found.");
        //         }

        //         var product = snapshot.ConvertTo<Products>();
        //         return ServiceResponse<Products>.CreateSuccess(product, "Product fetched successfully!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return ServiceResponse<Products>.CreateFailure($"Failed to fetch product: {ex.Message}");
        //     }
        // }

        // public async Task<ServiceResponse<string>> UpdateProduct(string branchId, string categoryId, string productId, Products updatedProduct)
        // {
        //     try
        //     {
        //         DocumentReference productDoc = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("categories")
        //             .Document(categoryId)
        //             .Collection("products")
        //             .Document(productId);

        //         var productUpdate = new Dictionary<string, object>
        //         {
        //             { "productName", updatedProduct.productName },
        //             { "description", updatedProduct.description },
        //             { "price", updatedProduct.price },
        //             { "stock", updatedProduct.stock }
        //         };

        //         await productDoc.SetAsync(productUpdate, SetOptions.MergeAll);
        //         return ServiceResponse<string>.CreateSuccess(productId, "Product updated successfully!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return ServiceResponse<string>.CreateFailure($"Failed to update product: {ex.Message}");
        //     }
        // }

        // public async Task<ServiceResponse<string>> DeleteProduct(string branchId, string categoryId, string productId)
        // {
        //     try
        //     {
        //         DocumentReference productDoc = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("categories")
        //             .Document(categoryId)
        //             .Collection("products")
        //             .Document(productId);

        //         await productDoc.DeleteAsync();
        //         return ServiceResponse<string>.CreateSuccess(productId, "Product deleted successfully!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return ServiceResponse<string>.CreateFailure($"Failed to delete product: {ex.Message}");
        //     }
        // }
        public async Task<ServiceResponse<string>> AddProduct(string branchId, Products product)
        {
            try
            {
                // สร้าง Product ID ใหม่
                string productId = await GetNextId($"product-sequence-{branchId}"); // ลำดับเฉพาะต่อ Branch

                var productDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                product.Id = productId;
                await productDoc.SetAsync(product);

                return ServiceResponse<string>.CreateSuccess(productId, "Product added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<object>>> GetProducts(string branchId)
        {
            try
            {
                CollectionReference products = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products");

                QuerySnapshot snapshot = await products.GetSnapshotAsync();
                var productsList = snapshot.Documents.Select(doc => new
                {
                    Id = doc.Id,
                    Data = doc.ToDictionary()
                }).Cast<object>().ToList();

                return ServiceResponse<List<object>>.CreateSuccess(productsList, "Products fetched successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<object>>.CreateFailure($"Failed to fetch products: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Products>> GetProductById(string branchId, string productId)
        {
            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                DocumentSnapshot snapshot = await productDoc.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return ServiceResponse<Products>.CreateFailure("Product not found.");
                }

                var product = snapshot.ConvertTo<Products>();
                return ServiceResponse<Products>.CreateSuccess(product, "Product fetched successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Products>.CreateFailure($"Failed to fetch product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Products>>> GetProductsByCategory(string branchId, string categoryId)
        {
            try
            {
                CollectionReference products = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products");

                var categoryDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("categories")
                    .Document(categoryId);
                var categorySnapshot = await categoryDoc.GetSnapshotAsync();

                if (!categorySnapshot.Exists)
                {
                    return ServiceResponse<List<Products>>.CreateFailure("Category not found.");
                }

                var query = products.WhereEqualTo("categoryId", categoryId);
                QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
                List<Products> productsList = querySnapshot.Documents.Select(doc => doc.ConvertTo<Products>()).ToList();

                return ServiceResponse<List<Products>>.CreateSuccess(productsList, "Products fetched successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Products>>.CreateFailure($"Failed to fetch products: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> UpdateProduct(string branchId, string productId, Products updatedProduct)
        {
            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                var productUpdate = new Dictionary<string, object>
                {
                    { "productName", updatedProduct.productName },
                    { "description", updatedProduct.description },
                    { "price", updatedProduct.price },
                    { "stock", updatedProduct.stock }
                };

                await productDoc.SetAsync(productUpdate, SetOptions.MergeAll);
                return ServiceResponse<string>.CreateSuccess(productId, "Product updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> UpdateStock(string branchId, string productId, Products product)
        {

            int quantity = product.quantity; // เข้าถึง quantity จากโมเดลที่ส่งมา

            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                // หาค่าสต็อกปัจจุบัน
                var productSnap = await productDoc.GetSnapshotAsync();
                if (!productSnap.Exists)
                {
                    return ServiceResponse<string>.CreateFailure("Product not found");
                }

                var existingStock = productSnap.GetValue<int>("stock");
                var newStock = existingStock - quantity; // ลดสต็อกตามที่ได้รับมา

                // ตั้งค่าการอัปเดต
                var productUpdate = new Dictionary<string, object>
                {
                    { "stock", newStock }
                };

                // อัปโหลดการอัปเดตลงไปใน Firestore
                await productDoc.SetAsync(productUpdate, SetOptions.MergeAll);
                return ServiceResponse<string>.CreateSuccess(productId, "Stock updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update stock: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> DeleteProduct(string branchId, string productId)
        {
            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                await productDoc.DeleteAsync();
                return ServiceResponse<string>.CreateSuccess(productId, "Product deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> DeleteAllProducts(string branchId)
        {
            try
            {
                var productsCollection = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products");

                var snapshots = await productsCollection.GetSnapshotAsync();
                var deleteTasks = snapshots.Documents.Select(doc => doc.Reference.DeleteAsync());

                await Task.WhenAll(deleteTasks); // ลบผลิตภัณฑ์ทั้งหมดในหมวดหมู่นั้น

                return ServiceResponse<string>.CreateSuccess(null, "All products deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete all products: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> ResetproductId(string branchId)
        {
            try
            {
                var sequenceDoc = _firestoreDb.Collection("config").Document($"product-sequence-{branchId}");
                await sequenceDoc.SetAsync(new { counter = 1 }); // รีเซ็ตค่าเป็น 1

                return ServiceResponse<string>.CreateSuccess("Branch ID sequence reset successfully!", "Reset done");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to reset branch ID sequence: {ex.Message}");
            }
        }
    }
}