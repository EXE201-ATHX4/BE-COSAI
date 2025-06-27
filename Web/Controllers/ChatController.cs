using Contract.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Service;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "API hỏi gimini", Description = "API này dùng để tạo đơn hàng mới từ phía client.")]
        public async Task<IActionResult> Chat([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            var userId = User.FindFirst("userId")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("Missing user ID in token.");

            var response = await _chatService.ProcessUserMessageAsync(int.Parse(userId), message);

            return Ok(new { success = true, response });
        }
    }
}
