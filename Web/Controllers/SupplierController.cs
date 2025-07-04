using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.SupplierModelViews;
using Sprache;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }
        [HttpGet]
        public async Task<ActionResult<BaseResponse<BasePaginatedList<SupplierModel>>>> GetSupplierAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {

                var suppliers = await _supplierService.GetAllSupplierAsync(pageNumber, pageSize);
                return suppliers.IsSuccess ? Ok(suppliers) : BadRequest(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<BasePaginatedList<SupplierModel>>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<SupplierModel>>> CreateSupplier([FromBody] CreateSupplierModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<SupplierModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _supplierService.CreateSupplierAsync(model, userId.Value);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<SupplierModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpPut]
        public async Task<ActionResult<BaseResponse<SupplierModel>>> UpdateSupplier(int supplierId, [FromBody] UpdateSupplierModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<SupplierModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _supplierService.UpdateSupplierAsync(supplierId, model, userId.Value);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<SupplierModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpDelete]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteSupplier(int supplierId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<bool>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _supplierService.DeleteSuppilerAsync(supplierId, userId.Value);
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
