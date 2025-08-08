using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.AIModelViews;
using ModelViews.OrderModelViews;
using Services.Service;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "API hỏi gimini")]
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
        [HttpGet]
        [SwaggerOperation(Summary = "API lấy lịch sử chat")]
        public async Task<ActionResult<BaseResponse<List<Chatresponse>>>> GetChat()
        {
            var userId = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized("Missing user ID in token");
            var response = await _chatService.ChatHistoryAsync(int.Parse(userId));
            if (!response.Any())
            {
                return new BaseResponse<List<Chatresponse>>(StatusCodeHelper.Notfound, "200", response, "don't have any conversation");
            }
            return Ok(new BaseResponse<List<Chatresponse>>(StatusCodeHelper.OK, "200", response, "Success"));
        }
    }
}
