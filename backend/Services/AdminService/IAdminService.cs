using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.AdminService
{
    public interface IAdminService
    {
        Task<string> AddBranch(Branch branch);
        Task<string> AddEmployee(string branchId, Employee employee);
        Task<string> AddProduct(string branchId, Products product);
        Task<List<object>> GetBranches();
        Task UpdateBranch(string branchId, Dictionary<string, object> updatedData);
        Task DeleteBranch(string branchId);
        Task<List<object>> GetEmployees(string branchId);
        Task UpdateEmployee(string branchId, string employeeId, Employee updatedEmployee);
        Task DeleteEmployee(string branchId, string employeeId);
        Task<List<object>> GetProducts(string branchId);
        Task UpdateProduct(string branchId, string productId, Products updatedProduct);
        Task DeleteProduct(string branchId, string productId);
    }
}