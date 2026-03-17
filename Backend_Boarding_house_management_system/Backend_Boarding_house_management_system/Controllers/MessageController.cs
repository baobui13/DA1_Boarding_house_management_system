using Backend_Boarding_house_management_system.DTOs.Message.Requests;
using Backend_Boarding_house_management_system.DTOs.Message.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        [HttpGet("GetMessageById")]
        public async Task<ActionResult<MessageResponse>> GetMessageById([FromQuery] GetMessageByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetMessagesByFilter")]
        public async Task<ActionResult<MessageListResponse>> GetMessagesByFilter([FromQuery] GetMessagesByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateMessage")]
        public async Task<ActionResult<MessageResponse>> CreateMessage([FromBody] CreateMessageRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("UpdateMessage")]
        public async Task<IActionResult> UpdateMessage([FromBody] UpdateMessageRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] DeleteMessageRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
