using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.UserModelViews;

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
    }
}
