using Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantDocumentController : ControllerBase
    {
        [HttpGet("GetTenantDocumentById")]
        public async Task<ActionResult<TenantDocumentResponse>> GetTenantDocumentById([FromQuery] GetTenantDocumentByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetTenantDocumentsByFilter")]
        public async Task<ActionResult<TenantDocumentListResponse>> GetTenantDocumentsByFilter([FromQuery] GetTenantDocumentsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateTenantDocument")]
        public async Task<ActionResult<TenantDocumentResponse>> CreateTenantDocument([FromBody] CreateTenantDocumentRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("UpdateTenantDocument")]
        public async Task<IActionResult> UpdateTenantDocument([FromBody] UpdateTenantDocumentRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteTenantDocument")]
        public async Task<IActionResult> DeleteTenantDocument([FromBody] DeleteTenantDocumentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
