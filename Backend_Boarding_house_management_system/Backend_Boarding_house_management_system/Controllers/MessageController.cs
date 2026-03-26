using Backend_Boarding_house_management_system.DTOs.Message.Requests;
using Backend_Boarding_house_management_system.DTOs.Message.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Services.Interfaces;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetMessageById")]
        public async Task<ActionResult<MessageResponse>> GetMessageById([FromQuery] GetMessageByIdRequest request)
        {
            var result = await _messageService.GetByIdAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetMessagesByFilter")]
        public async Task<ActionResult<MessageListResponse>> GetMessagesByFilter(
            [FromQuery] EntityFilter<Message> filter,
            [FromQuery] EntitySort<Message> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _messageService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpPost("CreateMessage")]
        public async Task<ActionResult<MessageResponse>> CreateMessage([FromBody] CreateMessageRequest request)
        {
            var result = await _messageService.CreateAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpPut("UpdateMessage")]
        public async Task<IActionResult> UpdateMessage([FromBody] UpdateMessageRequest request)
        {
            await _messageService.UpdateAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpDelete("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] DeleteMessageRequest request)
        {
            await _messageService.DeleteAsync(request);
            return Ok();
        }
    }
}
