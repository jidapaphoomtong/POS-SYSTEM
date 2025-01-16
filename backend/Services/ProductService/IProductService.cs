using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.ProductService
{
    public interface IProductService
    {
        Task<string> GetNextId(string sequenceName);
        Task<ServiceResponse<string>> AddProduct(string branchId, string categoryId, Products product);
        Task<ServiceResponse<List<object>>> GetProducts(string branchId, string categoryId);
        Task<ServiceResponse<Products>> GetProductById(string branchId, string categoryId, string productId);
        Task<ServiceResponse<List<Products>>> GetProductsByCategory(string branchId, string categoryId);
        Task<ServiceResponse<string>> UpdateProduct(string branchId, string categoryId, string productId, Products updatedProduct);
        Task<ServiceResponse<string>> DeleteProduct(string branchId, string categoryId, string productId);
        Task<ServiceResponse<string>> DeleteAllProducts(string branchId, string categoryId);
        Task<ServiceResponse<string>> ResetproductId(string branchId);
    }
}