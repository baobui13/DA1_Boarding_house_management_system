using Backend_Boarding_house_management_system.DTOs.TenantDocument.Requests;
using Backend_Boarding_house_management_system.DTOs.TenantDocument.Responses;
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
    public class TenantDocumentController : ControllerBase
    {
        private readonly ITenantDocumentService _tenantDocumentService;

        public TenantDocumentController(ITenantDocumentService tenantDocumentService)
        {
            _tenantDocumentService = tenantDocumentService;
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetTenantDocumentById")]
        public async Task<ActionResult<TenantDocumentResponse>> GetTenantDocumentById([FromQuery] GetTenantDocumentByIdRequest request)
        {
            var result = await _tenantDocumentService.GetByIdAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetTenantDocumentsByFilter")]
        public async Task<ActionResult<TenantDocumentListResponse>> GetTenantDocumentsByFilter(
            [FromQuery] EntityFilter<TenantDocument> filter,
            [FromQuery] EntitySort<TenantDocument> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _tenantDocumentService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPost("CreateTenantDocument")]
        public async Task<ActionResult<TenantDocumentResponse>> CreateTenantDocument([FromBody] CreateTenantDocumentRequest request)
        {
            var result = await _tenantDocumentService.CreateAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPut("UpdateTenantDocument")]
        public async Task<IActionResult> UpdateTenantDocument([FromBody] UpdateTenantDocumentRequest request)
        {
            await _tenantDocumentService.UpdateAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpDelete("DeleteTenantDocument")]
        public async Task<IActionResult> DeleteTenantDocument([FromBody] DeleteTenantDocumentRequest request)
        {
            await _tenantDocumentService.DeleteAsync(request);
            return Ok();
        }
    }
}
