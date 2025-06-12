using Contract.Services.Interface;
using Core.Base;
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
        public async Task<ActionResult<BasePaginatedList<UserModelResponse>>> GetAllAccounts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var accounts = await _accountService.GetAllAccounts(pageNumber, pageSize);
                Console.WriteLine("HAHAHAAHAHA");
                return Ok(BaseResponse<BasePaginatedList<UserModelResponse>>.OkResponse(accounts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving Accounts: {ex.Message}");
            }
        }
    }
}
