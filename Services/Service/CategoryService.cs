using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelViews.CategoryModelViews;
namespace Services.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.GetRepository<Category>().Entities
                    .Include(c => c.SubCategory)
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null) return null;
                
                if (category.ParentId.HasValue)
                {
                    var parent = category.ParentId.HasValue
                    ? await _unitOfWork.GetRepository<Category>().Entities
                        .FirstOrDefaultAsync(x => x.Id == category.ParentId.Value)
                    : null;
                }

                return new CategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ParentId = category.ParentId,
                    Level = category.Level,
                    DisplayOrder = category.DisplayOrder,
                    IsActive = category.IsActive,
                    AllowProducts = category.AllowProducts,
                    ShowInMenu = category.ShowInMenu,
                    ProductCount = category.Products?.Count ?? 0,
                    SubCategories = category.SubCategory?.Select(sc => new CategoryModel
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Description = sc.Description,
                        Level = sc.Level,
                        DisplayOrder = sc.DisplayOrder,
                        IsActive = sc.IsActive,
                        ProductCount = sc.Products?.Count ?? 0
                    }).OrderBy(sc => sc.DisplayOrder).ToList() ?? new List<CategoryModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by id: {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryModel> CreateCategoryAsync(CreateCategoryModel model)
        {
            try
            {
                // Validate parent category if specified
                if (model.ParentId.HasValue && !await ValidateParentCategoryAsync(0, model.ParentId))
                {
                    throw new ArgumentException("Invalid parent category");
                }

                // Calculate level
                int level = 0;
                if (model.ParentId.HasValue)
                {
                    var parent = await _unitOfWork.GetRepository<Category>().Entities
                        .FirstOrDefaultAsync(c => c.Id == model.ParentId.Value);
                    if (parent != null)
                    {
                        level = parent.Level + 1;
                    }
                }

                // Check for duplicate name at same level
                var existingCategory = await _unitOfWork.GetRepository<Category>().Entities
                    .FirstOrDefaultAsync(c => c.Name == model.Name && c.ParentId == model.ParentId);

                if (existingCategory != null)
                {
                    throw new ArgumentException("Category with this name already exists at this level");
                }

                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    ParentId = model.ParentId,
                    Level = level,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    AllowProducts = model.AllowProducts,
                    ShowInMenu = model.ShowInMenu
                };

                await _unitOfWork.GetRepository<Category>().InsertAsync(category);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Created category: {CategoryName} with ID: {CategoryId}",
                    category.Name, category.Id);

                return await GetCategoryByIdAsync(category.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", model.Name);
                throw;
            }
        }

        public async Task<bool> ValidateParentCategoryAsync(int categoryId, int? parentId)
        {
            try
            {
                if (!parentId.HasValue) return true;

                // Check if parent exists
                var parent = await _unitOfWork.GetRepository<Category>().Entities
                    .FirstOrDefaultAsync(c => c.Id == parentId.Value);

                if (parent == null) return false;

                // Check for circular reference (category cannot be its own ancestor)
                if (categoryId > 0)
                {
                    var currentParentId = parentId;
                    while (currentParentId.HasValue)
                    {
                        if (currentParentId.Value == categoryId)
                        {
                            return false; // Circular reference detected
                        }

                        var currentParent = await _unitOfWork.GetRepository<Category>().Entities
                            .FirstOrDefaultAsync(c => c.Id == currentParentId.Value);

                        currentParentId = currentParent?.ParentId;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating parent category: {ParentId}", parentId);
                return false;
            }
        }

        public async Task<CategoryModel?> UpdateCategoryAsync(int id, UpdateCategoryModel model)
        {
            try
            {
                var category = await _unitOfWork.GetRepository<Category>().Entities
                    .FirstOrDefaultAsync(c => c.Id == id);  

                if (category == null) return null;

                // Validate parent category change
                if (model.ParentId != category.ParentId)
                {
                    if (!await ValidateParentCategoryAsync(id, model.ParentId))
                    {
                        throw new ArgumentException("Invalid parent category or circular reference detected");
                    }
                }

                // Check for duplicate name (excluding current category)
                var existingCategory = await _unitOfWork.GetRepository<Category>().Entities
                    .FirstOrDefaultAsync(c => c.Name == model.Name &&
                                            c.ParentId == model.ParentId &&
                                            c.Id != id);

                if (existingCategory != null)
                {
                    throw new ArgumentException("Category with this name already exists at this level");
                }

                // Update level if parent changed
                if (model.ParentId != category.ParentId)
                {
                    int newLevel = 0;
                    if (model.ParentId.HasValue)
                    {
                        var parent = await _unitOfWork.GetRepository<Category>().Entities
                            .FirstOrDefaultAsync(c => c.Id == model.ParentId.Value);
                        if (parent != null)
                        {
                            newLevel = parent.Level + 1;
                        }
                    }

                    // Update level for this category and all descendants
                    await UpdateCategoryLevelsRecursiveAsync(id, newLevel);
                }

                category.Name = model.Name;
                category.Description = model.Description;
                category.ParentId = model.ParentId;
                category.DisplayOrder = model.DisplayOrder;
                category.IsActive = model.IsActive;
                category.AllowProducts = model.AllowProducts;
                category.ShowInMenu = model.ShowInMenu;

                _unitOfWork.GetRepository<Category>().Update(category);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Updated category: {CategoryId}", id);

                return await GetCategoryByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                throw;
            }
        }

        private async Task UpdateCategoryLevelsRecursiveAsync(int categoryId, int newLevel)
        {
            var category = await _unitOfWork.GetRepository<Category>().Entities
                .Include(c => c.SubCategory)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null) return;

            category.Level = newLevel;
            _unitOfWork.GetRepository<Category>().Update(category);

            // Update all children recursively
            foreach (var child in category.SubCategory)
            {
                await UpdateCategoryLevelsRecursiveAsync(child.Id, newLevel + 1);
            }
        }

        public async Task<bool> CanDeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.GetRepository<Category>().Entities
                    .Include(c => c.SubCategory)
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null) return false;

                // Cannot delete if has subcategories or products
                return !category.SubCategory.Any() && !category.Products.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category can be deleted: {CategoryId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                if (!await CanDeleteCategoryAsync(id))
                {
                    return false;
                }

                var category = await _unitOfWork.GetRepository<Category>().Entities
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null) return false;

                _unitOfWork.GetRepository<Category>().Delete(category);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Deleted category: {CategoryId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return false;
            }
        }

        public async Task<bool> MoveCategoryAsync(int categoryId, int? newParentId, int newDisplayOrder)
        {
            try
            {
                var category = await _unitOfWork.GetRepository<Category>().Entities
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                if (category == null) return false;

                // Validate new parent
                if (!await ValidateParentCategoryAsync(categoryId, newParentId))
                {
                    return false;
                }

                // Calculate new level
                int newLevel = 0;
                if (newParentId.HasValue)
                {
                    var parent = await _unitOfWork.GetRepository<Category>().Entities
                        .FirstOrDefaultAsync(c => c.Id == newParentId.Value);
                    if (parent != null)
                    {
                        newLevel = parent.Level + 1;
                    }
                }

                // Update category
                category.ParentId = newParentId;
                category.DisplayOrder = newDisplayOrder;

                // Update levels if parent changed
                if (category.Level != newLevel)
                {
                    await UpdateCategoryLevelsRecursiveAsync(categoryId, newLevel);
                }

                _unitOfWork.GetRepository<Category>().Update(category);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Moved category: {CategoryId} to parent: {ParentId}",
                    categoryId, newParentId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving category: {CategoryId}", categoryId);
                return false;
            }
        }

        public async Task<List<CategoryModel>> GetBreadcrumbAsync(int categoryId)
        {
            try
            {
                var breadcrumb = new List<CategoryModel>();
                int? currentId = categoryId; // Change type to nullable int

                while (currentId.HasValue) // Fix: Use nullable int to check for HasValue
                {
                    var category = await _unitOfWork.GetRepository<Category>().Entities
                        .FirstOrDefaultAsync(c => c.Id == currentId.Value); // Access Value property of nullable int

                    if (category == null) break;

                    breadcrumb.Insert(0, new CategoryModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Level = category.Level,
                        ParentId = category.ParentId
                    });

                    currentId = category.ParentId; // Update currentId with ParentId
                }

                return breadcrumb;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting breadcrumb for category: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<List<CategoryModel>> GetActiveCategoriesAsync()
        {
            try
            {
                return await _unitOfWork.GetRepository<Category>().Entities
                    .Include(c => c.Products)
                    .Where(c => c.IsActive && c.ShowInMenu)
                    .OrderBy(c => c.Level)
                    .ThenBy(c => c.DisplayOrder)
                    .Select(c => new CategoryModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ParentId = c.ParentId,
                        Level = c.Level,
                        DisplayOrder = c.DisplayOrder,
                        IsActive = c.IsActive,
                        AllowProducts = c.AllowProducts,
                        ShowInMenu = c.ShowInMenu,
                        ProductCount = c.Products.Count
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active categories");
                throw;
            }
        }

        public async Task<List<CategoryModel>> GetCategoriesByParentIdAsync(int? parentId)
        {
            try
            {
                return await _unitOfWork.GetRepository<Category>().Entities
                    .Include(c => c.Products)
                    .Where(c => c.ParentId == parentId && c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new CategoryModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Level = c.Level,
                        DisplayOrder = c.DisplayOrder,
                        IsActive = c.IsActive,
                        AllowProducts = c.AllowProducts,
                        ShowInMenu = c.ShowInMenu,
                        ProductCount = c.Products.Count
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories by parent: {ParentId}", parentId);
                throw;
            }
        }
    }
}
