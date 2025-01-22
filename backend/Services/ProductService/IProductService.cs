using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.ProductService
{
    public interface IProductService
    {
        Task<string> GetNextId(string sequenceName);
        Task<ServiceResponse<string>> AddProduct(string branchId, Products product);
        Task<ServiceResponse<List<object>>> GetProducts(string branchId);
        Task<ServiceResponse<Products>> GetProductById(string branchId, string productId);
        Task<ServiceResponse<List<Products>>> GetProductsByCategory(string branchId, string categoryId);
        Task<ServiceResponse<string>> UpdateProduct(string branchId, string productId, Products updatedProduct);
        Task<bool> UpdateProductStatusAsync(string branchId, string productId, string status);
        Task<ServiceResponse<string>> UpdateStock(string branchId, string productId, Products product);
        Task<ServiceResponse<string>> DeleteProduct(string branchId, string productId);
        Task<ServiceResponse<string>> DeleteAllProducts(string branchId);
        Task<ServiceResponse<string>> ResetproductId(string branchId);
    }
}