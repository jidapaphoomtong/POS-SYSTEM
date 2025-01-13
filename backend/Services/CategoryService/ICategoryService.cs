using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<ServiceResponse<string>> AddCategory(string branchId, Category category);
        Task<ServiceResponse<List<Category>>> GetAllCategories(string branchId);
        Task<ServiceResponse<Category>> GetCategoryById(string branchId, string categoryId);
        Task<ServiceResponse<string>> UpdateCategory(string branchId, Category category);
        Task<ServiceResponse<string>> DeleteCategory(string branchId, string categoryId);
    }
}