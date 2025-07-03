using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Mvc;
using ModelViews.CategoryModelViews;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryModel>>> GetCategories([FromQuery] int? parentId = null)
        {
            try
            {
                var categories = await _categoryService.GetCategoriesByParentIdAsync(parentId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("active")]
        public async Task<ActionResult<List<CategoryModel>>> GetActiveCategories()
        {
            try
            {
                var categories = await _categoryService.GetActiveCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active categories");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryModel>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/breadcrumb")]
        public async Task<ActionResult<List<CategoryModel>>> GetBreadcrumb(int id)
        {
            try
            {
                var breadcrumb = await _categoryService.GetBreadcrumbAsync(id);
                return Ok(breadcrumb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting breadcrumb for category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CategoryModel>> CreateCategory([FromBody] CreateCategoryModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = await _categoryService.CreateCategoryAsync(model);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryModel>> UpdateCategory(int id, [FromBody] UpdateCategoryModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = await _categoryService.UpdateCategoryAsync(id, model);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                if (!await _categoryService.CanDeleteCategoryAsync(id))
                    return BadRequest("Cannot delete category that has subcategories or products");

                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/move")]
        public async Task<ActionResult> MoveCategory(int id, [FromBody] MoveCategoryModel model)
        {
            try
            {
                var result = await _categoryService.MoveCategoryAsync(id, model.NewParentId, model.NewDisplayOrder);
                if (!result)
                    return BadRequest("Cannot move category");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/can-delete")]
        public async Task<ActionResult<bool>> CanDeleteCategory(int id)
        {
            try
            {
                var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
                return Ok(canDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category can be deleted {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
