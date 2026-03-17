using Backend_Boarding_house_management_system.DTOs.Invoice.Requests;
using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        [HttpGet("GetInvoiceById")]
        public async Task<ActionResult<InvoiceResponse>> GetInvoiceById([FromQuery] GetInvoiceByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetInvoicesByFilter")]
        public async Task<ActionResult<InvoiceListResponse>> GetInvoicesByFilter([FromQuery] GetInvoicesByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateInvoice")]
        public async Task<ActionResult<InvoiceResponse>> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice([FromBody] UpdateInvoiceRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteInvoice")]
        public async Task<IActionResult> DeleteInvoice([FromBody] DeleteInvoiceRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
