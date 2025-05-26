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
    public class CateogoryService : ICateogoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CateogoryService> _logger;
        public CateogoryService(IUnitOfWork unitOfWork, ILogger<CateogoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<bool> CreateCategory(CreateCategoryModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to create category with name: {Name}", model.Name);

                var existingCategory = await _unitOfWork.GetRepository<Category>()
                    .Entities
                    .FirstOrDefaultAsync(b => b.Name.ToLower() == model.Name.ToLower() && !b.DeletedTime.HasValue);

                if (existingCategory != null)
                {
                    return false; // Category already exists
                }

                var category = new Category
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    CreatedBy = "System",
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.GetRepository<Category>().InsertAsync(category);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Successfully created category with ID: {Id}", category.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create BandBrand: {Message}", ex.Message);
                throw;
            }
            return true;
        }
    }
}
