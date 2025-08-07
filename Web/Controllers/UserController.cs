using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.UserModelViews;
using Services.Service;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _accountService;

        public UserController(IUserService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet]
        public async Task<BaseResponse<BasePaginatedList<UserModelResponse>>> GetAllAccounts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var accounts = await _accountService.GetAllAccounts(pageNumber, pageSize);
                //Console.WriteLine("HAHAHAAHAHA");
                return new BaseResponse<BasePaginatedList<UserModelResponse>>(StatusCodeHelper.OK, "200", accounts);
            }
            catch (Exception ex)
            {
                return new BaseResponse<BasePaginatedList<UserModelResponse>>(StatusCodeHelper.ServerError, "500", $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BaseResponse<UserInfoModel>>> CreateUserInfo([FromBody] CreateUserInfo model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<UserInfoModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _accountService.CreateInfoModelAsync(model, userId.Value);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<UserInfoModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
            }
        }
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<BaseResponse<UserInfoModel>>> UpdateUserInfo([FromBody] UserInfoModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest(new BaseResponse<UserInfoModel>(StatusCodeHelper.Notfound, "400", "Invalid user"));
                }
                var result = await _accountService.UpdateUserInfotAsync(model, userId.Value);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<UserInfoModel>(StatusCodeHelper.ServerError, "500", "Internal server error"));
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
