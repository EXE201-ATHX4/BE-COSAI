using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelViews.CategoryModelViews;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICateogoryService _categoryService;
        public CategoryController(ICateogoryService categoryService)
        {
            _categoryService = categoryService;
        }
        // GET: api/<CategoryController>
        [HttpGet]
        [Authorize]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CategoryController>
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    throw new ArgumentException("Name is required and cannot be empty or whitespace.");
                }

                if (model.Name.Length > 255)
                {
                    throw new ArgumentException("Name cannot exceed 255 characters.");
                }
                var cate = await _categoryService.CreateCategory(model);
                return !cate ? BadRequest("Category already exists") : Ok("Create successful"); // return cate created
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
