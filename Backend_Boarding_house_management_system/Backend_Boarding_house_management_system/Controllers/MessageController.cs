using Backend_Boarding_house_management_system.DTOs.Message.Requests;
using Backend_Boarding_house_management_system.DTOs.Message.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetMessageById")]
        public async Task<ActionResult<MessageResponse>> GetMessageById([FromQuery] GetMessageByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetMessagesByFilter")]
        public async Task<ActionResult<MessageListResponse>> GetMessagesByFilter([FromQuery] GetMessagesByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpPost("CreateMessage")]
        public async Task<ActionResult<MessageResponse>> CreateMessage([FromBody] CreateMessageRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpPut("UpdateMessage")]
        public async Task<IActionResult> UpdateMessage([FromBody] UpdateMessageRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpDelete("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] DeleteMessageRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
