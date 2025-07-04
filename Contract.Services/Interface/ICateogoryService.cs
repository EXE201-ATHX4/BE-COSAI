using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract.Repositories.Entity;
using ModelViews.CategoryModelViews;

namespace Contract.Services.Interface
{
    public interface ICategoryService
    {
        //GET
        //CREATE
        //UPDATE
        //DELETE

        //Task<PagedResult<CategoryModel>> GetCategoriesAsync(int page = 1, int pageSize = 10, string? search = null, int? parentId = null);
        Task<CategoryModel?> GetCategoryByIdAsync(int id);
        Task<CategoryModel> CreateCategoryAsync(CreateCategoryModel model);
        Task<CategoryModel?> UpdateCategoryAsync(int id, UpdateCategoryModel model);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> MoveCategoryAsync(int categoryId, int? newParentId, int newDisplayOrder);
        Task<List<CategoryModel>> GetBreadcrumbAsync(int categoryId); //Danh sách các danh mục từ gốc đến danh mục hiện tạ
        Task<bool> CanDeleteCategoryAsync(int id);
        Task<List<CategoryModel>> GetActiveCategoriesAsync();
        Task<bool> ValidateParentCategoryAsync(int categoryId, int? parentId);
        Task<List<CategoryModel>> GetCategoriesByParentIdAsync(int? parentId);

    }
}
