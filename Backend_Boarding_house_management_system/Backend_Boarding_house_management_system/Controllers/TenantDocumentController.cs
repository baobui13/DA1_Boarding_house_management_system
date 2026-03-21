using Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TenantDocumentController : ControllerBase
    {
        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetTenantDocumentById")]
        public async Task<ActionResult<TenantDocumentResponse>> GetTenantDocumentById([FromQuery] GetTenantDocumentByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetTenantDocumentsByFilter")]
        public async Task<ActionResult<TenantDocumentListResponse>> GetTenantDocumentsByFilter([FromQuery] GetTenantDocumentsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPost("CreateTenantDocument")]
        public async Task<ActionResult<TenantDocumentResponse>> CreateTenantDocument([FromBody] CreateTenantDocumentRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPut("UpdateTenantDocument")]
        public async Task<IActionResult> UpdateTenantDocument([FromBody] UpdateTenantDocumentRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpDelete("DeleteTenantDocument")]
        public async Task<IActionResult> DeleteTenantDocument([FromBody] DeleteTenantDocumentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
