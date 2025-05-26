using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelViews.CategoryModelViews;

namespace Contract.Services.Interface
{
    public interface ICateogoryService
    {
        Task<bool> CreateCategory(CreateCategoryModel model);
        //Task<bool> UpdateCategory(UpdateCategoryModel model);
        //Task<bool> DeleteCategory(int id);
        //Task<CategoryModel> GetCategoryById(int id);
        //Task<List<CategoryModel>> GetAllCategories();
    }
}
