using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.NotificationService;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace backend.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly FirestoreDB _firestoreDb;
        private readonly INotificationService _notificationService; // เพิ่มตัวแปรนี้

        public ProductService(FirestoreDB firestoreDb, INotificationService notificationService) // เพิ่ม Parameter
    {
        _firestoreDb = firestoreDb;
        _notificationService = notificationService; // กำหนดค่า
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

        public async Task<ServiceResponse<string>> AddProduct(string branchId, Products product)
        {
            try
            {
                // ตรวจสอบว่า stock ตรงตามข้อกำหนด หรือไม่
                if (product.stock < product.reorderPoint)
                {
                    return ServiceResponse<string>.CreateFailure("Stock cannot be below the reorder point.");
                }

                string productId = await GetNextId($"product-sequence-{branchId}");
                var productDoc = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products)
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
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products);

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
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products)
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
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products);

                var categoryDoc = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Categories)
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
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products)
                    .Document(productId);
                
                var currentProduct = await productDoc.GetSnapshotAsync();
                if (!currentProduct.Exists) return ServiceResponse<string>.CreateFailure("Product not found");

                var currentStock = currentProduct.GetValue<int>("stock");
                var updates = new Dictionary<string, object>
                {
                    { "productName", updatedProduct.productName },
                    { "description", updatedProduct.description },
                    { "price", updatedProduct.price },
                    { "stock", updatedProduct.stock },
                    { "reorderPoint", updatedProduct.reorderPoint }
                };

                // ตรวจสอบ Stock และ Trigger Notifications
                await CheckStockAndNotify(branchId, productId, updatedProduct.stock, currentStock, updatedProduct.reorderPoint);

                await productDoc.SetAsync(updates, SetOptions.MergeAll);
                return ServiceResponse<string>.CreateSuccess(productId, "Product updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update product: {ex.Message}");
            }
        }

        private async Task CheckStockAndNotify(string branchId, string productId, int newStock, int currentStock, int reorderPoint)
        {
            if (newStock < reorderPoint && currentStock >= reorderPoint)
            {
                await _notificationService.NotifyLowStock(branchId, productId, newStock);
            }
            if (newStock <= 0)
            {
                await UpdateProductStatusAsync(branchId, productId, "inactive");
                await _notificationService.NotifyLowStock(branchId, productId, newStock);
            }
            else if (newStock > 0)
            {
                await UpdateProductStatusAsync(branchId, productId, "active");
            }
        }

        public async Task<bool> UpdateProductStatusAsync(string branchId, string productId, string status)
        {
            var productRef = _firestoreDb
                .Collection(FirestoreCollections.Branches)
                .Document(branchId)
                .Collection(FirestoreCollections.Products)
                .Document(productId);

            var productSnapshot = await productRef.GetSnapshotAsync();

            if (!productSnapshot.Exists)
            {
                return false; // ไม่พบผลิตภัณฑ์
            }

            try
            {
                // สร้าง Dictionary สำหรับการอัปเดตสถานะ
                var updates = new Dictionary<string, object>
                {
                    { "status", status }
                };

                // อัปเดตสถานะ
                await productRef.UpdateAsync(updates);
                return true; // อัปเดตสำเร็จ
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product status: {ex.Message}");
                return false; // การอัปเดตล้มเหลว
            }
        }

        public async Task<ServiceResponse<string>> UpdateStock(string branchId, string productId, Products product)
        {
            int quantity = product.quantity;

            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products)
                    .Document(productId);

                var productSnap = await productDoc.GetSnapshotAsync();
                if (!productSnap.Exists)
                {
                    return ServiceResponse<string>.CreateFailure("Product not found");
                }

                var existingStock = productSnap.GetValue<int>("stock");
                var newStock = existingStock - quantity;
                var reorderPoint = productSnap.GetValue<int>("reorderPoint");

                var updates = new Dictionary<string, object>
                {
                    { "stock", newStock }
                };

                // ตรวจสอบและส่งการแจ้งเตือนสำหรับระดับต่ำ
                if (newStock < reorderPoint && existingStock >= reorderPoint)
                {
                    await _notificationService.NotifyLowStock(branchId, productId, newStock);
                }

                // ตรวจสอบการหมดสต็อก
                if (newStock <= 0)
                {
                    updates["status"] = "inactive"; // ตั้งสถานะเป็น inactive
                    await _notificationService.NotifyOutOfStock(branchId, productId); // ไม่ต้องส่ง newStock เพราะไม่ได้ใช้
                }
                else
                {
                    await UpdateProductStatusAsync(branchId, productId, "active"); // เปลี่ยนสถานะเป็น active
                }

                await productDoc.SetAsync(updates, SetOptions.MergeAll);
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
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products)
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
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Products);

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
                var sequenceDoc = _firestoreDb.Collection(FirestoreCollections.Config).Document($"product-sequence-{branchId}");
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