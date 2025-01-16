using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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

        public async Task<ServiceResponse<string>> AddProduct(string branchId, Products product)
        {
            try
            {
                // ตรวจสอบว่า categoryId มีอยู่
                var categoryDoc = _firestoreDb.Collection("branches").Document(branchId)
                                .Collection("categories").Document(product.categoryId);
                var snapshot = await categoryDoc.GetSnapshotAsync();
                
                if (!snapshot.Exists)
                {
                    return ServiceResponse<string>.CreateFailure("Category does not exist.");
                }

                // สร้าง Product ID ใหม่
                string productId = await GetNextId($"product-sequence-{branchId}"); // ลำดับเฉพาะต่อ Branch

                // อ้างถึง Document ID
                DocumentReference productDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                // เพิ่ม Product ID เข้าไปใน Object ก่อนบันทึก
                product.Id = productId;

                // บันทึก Product ลงใน Firestore
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

        // public async Task<ServiceResponse<List<Products>>> GetProductsByCategory(string branchId, string categoryId)
        // {
        //     try
        //     {
        //         CollectionReference products = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("products");

        //         // ตรวจสอบว่า categoryId มีอยู่หรือไม่
        //         var categoryDoc = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("categories")
        //             .Document(categoryId);
                
        //         var categorySnapshot = await categoryDoc.GetSnapshotAsync();
        //         if (!categorySnapshot.Exists)
        //         {
        //             return ServiceResponse<List<Products>>.CreateFailure("Category not found.");
        //         }

        //         // ใช้ Where เพื่อกรองผลิตภัณฑ์ตาม categoryId
        //         var query = products.Where("categoryId", "==", categoryId); // ใช้ 2 อาร์กิวเมนต์
        //         QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        //         List<Products> productsList = querySnapshot.Documents.Select(doc => doc.ConvertTo<Products>()).ToList();
        //         return ServiceResponse<List<Products>>.CreateSuccess(productsList, "Products fetched successfully!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return ServiceResponse<List<Products>>.CreateFailure($"Failed to fetch products: {ex.Message}");
        //     }
        // }

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
                    { "stock", updatedProduct.stock } // สามารถใช้ "∞" หรือค่าที่ต้องการ
                };

                await productDoc.SetAsync(productUpdate, SetOptions.MergeAll); // ใช้ Merge เพื่ออัปเดตข้อมูลที่มีอยู่

                return ServiceResponse<string>.CreateSuccess(productId, "Product updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> SellProduct(string branchId, string productId, int quantity)
        {
            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                var snapshot = await productDoc.GetSnapshotAsync();
                var currentStock = snapshot.GetValue<int>("stock");

                if (currentStock < quantity)
                {
                    return ServiceResponse<string>.CreateFailure("Not enough stock available.");
                }

                // ลดจำนวน Stock
                await productDoc.UpdateAsync(new Dictionary<string, object>
                {
                    { "stock", currentStock - quantity }
                });

                return ServiceResponse<string>.CreateSuccess(productId, "Product sold and stock updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to sell product: {ex.Message}");
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

        public async Task<ServiceResponse<string>> AddStock(string branchId, string productId, int quantity)
        {
            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                var snapshot = await productDoc.GetSnapshotAsync();
                if (!snapshot.Exists)
                {
                    return ServiceResponse<string>.CreateFailure("Product not found.");
                }

                var currentStock = snapshot.GetValue<int>("stock");
                await productDoc.UpdateAsync(new Dictionary<string, object>
                {
                    { "stock", currentStock + quantity }
                });

                return ServiceResponse<string>.CreateSuccess(productId, "Stock added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add stock: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> ReduceStock(string branchId, string productId, int quantity)
        {
            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                var snapshot = await productDoc.GetSnapshotAsync();
                if (!snapshot.Exists)
                {
                    return ServiceResponse<string>.CreateFailure("Product not found.");
                }

                var currentStock = snapshot.GetValue<int>("stock");
                if (currentStock < quantity)
                {
                    return ServiceResponse<string>.CreateFailure("Not enough stock available.");
                }

                await productDoc.UpdateAsync(new Dictionary<string, object>
                {
                    { "stock", currentStock - quantity }
                });

                return ServiceResponse<string>.CreateSuccess(productId, "Stock reduced successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to reduce stock: {ex.Message}");
            }
        }
    }
}