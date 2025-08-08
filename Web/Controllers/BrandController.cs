using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Microsoft.AspNetCore.Mvc;
using ModelViews.BrandModelViews;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }
        [HttpGet]
        public async Task<ActionResult<BaseResponse<BasePaginatedList<BrandModel>>>> GetBrandAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {

                var brands = await _brandService.GetAllBrandAsync(pageNumber, pageSize);
                return (bool)brands.IsSuccess ? Ok(brands) : BadRequest(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<BasePaginatedList<BrandModel>>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<BrandModel>>> CreateBrand([FromBody] CreateBrandModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<BrandModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _brandService.CreateBrandAsync(model, userId.Value);
                return (bool)result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<BrandModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpPut]
        public async Task<ActionResult<BaseResponse<BrandModel>>> UpdateBrand(int BrandId, [FromBody] UpdateBrandModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<BrandModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _brandService.UpdateBrandAsync(BrandId, model, userId.Value);
                return (bool)result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<BrandModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpDelete]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteBrand(int brandId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<bool>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _brandService.DeleteBrandAsync(brandId, userId.Value);
                return (bool)result.IsSuccess ? Ok(result) : BadRequest(result);
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
