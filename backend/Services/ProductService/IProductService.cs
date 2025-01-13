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
        Task<ServiceResponse<string>> AddProduct(string branchId, Products product);
        Task<ServiceResponse<List<object>>> GetProducts(string branchId);
        Task<ServiceResponse<Products>> GetProductById(string branchId, string productId);
        Task<ServiceResponse<string>> SellProduct(string branchId, string productId, int quantity);
        Task<ServiceResponse<string>> UpdateProduct(string branchId, string productId, Products updatedProduct);
        Task<ServiceResponse<string>> DeleteProduct(string branchId, string productId);
        Task<ServiceResponse<string>> AddStock(string branchId, string productId, int quantity);
        Task<ServiceResponse<string>> ReduceStock(string branchId, string productId, int quantity);
    }
}