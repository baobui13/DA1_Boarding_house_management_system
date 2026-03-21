using Backend_Boarding_house_management_system.DTOs.Invoice.Requests;
using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetInvoiceById")]
        public async Task<ActionResult<InvoiceResponse>> GetInvoiceById([FromQuery] GetInvoiceByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetInvoicesByFilter")]
        public async Task<ActionResult<InvoiceListResponse>> GetInvoicesByFilter([FromQuery] GetInvoicesByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreateInvoice")]
        public async Task<ActionResult<InvoiceResponse>> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice([FromBody] UpdateInvoiceRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteInvoice")]
        public async Task<IActionResult> DeleteInvoice([FromBody] DeleteInvoiceRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
