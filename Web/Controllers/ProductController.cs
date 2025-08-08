using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Microsoft.AspNetCore.Mvc;
using ModelViews.ProductModelViews;
using System.Security.Claims;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        /// <summary>
        /// lấy all product
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<BasePaginatedList<ProductModel>>>> GetProductAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? productId = null, [FromQuery] int? supplierId = null, [FromQuery] int? brandId = null, [FromQuery] int? categoryId = null, [FromQuery] string? name = null)
        {
            try
            {

                var products = await _productService.GetAllProductAsync(pageNumber, pageSize, productId, supplierId, brandId, categoryId, name);
                return products.IsSuccess ? Ok(products) : BadRequest(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<BasePaginatedList<ProductModel>>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        /// <summary>
        /// Get product by Id
        /// </summary>
        [HttpGet("{productId}")]
        public async Task<ActionResult<BaseResponse<ProductModel>>> GetProductByIdAsync([FromRoute] int productId)
        {
            try
            {

                var products = await _productService.GetProductByIdAsync(productId);
                return products.IsSuccess ? Ok(products) : BadRequest(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ProductModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpPost]
        public async Task<ActionResult<BaseResponse<ProductModel>>> Createproduct([FromBody] CreateProductModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<ProductModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _productService.CreateProductAsync(model, userId.Value);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ProductModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpPut]
        public async Task<ActionResult<BaseResponse<ProductModel>>> Updateproduct(int productId, [FromBody] UpdateProductModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<ProductModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _productService.UpdateProductAsync(productId, model, userId.Value);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<ProductModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpDelete]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteProduct(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<bool>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _productService.DeleteProductAsync(productId, userId.Value);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
