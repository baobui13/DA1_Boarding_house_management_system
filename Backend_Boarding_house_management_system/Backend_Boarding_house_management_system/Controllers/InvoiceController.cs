using Backend_Boarding_house_management_system.DTOs.Invoice.Requests;
using Backend_Boarding_house_management_system.DTOs.Invoice.Responses;
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
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetInvoiceById")]
        public async Task<ActionResult<InvoiceResponse>> GetInvoiceById([FromQuery] GetInvoiceByIdRequest request)
        {
            var result = await _invoiceService.GetByIdAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetInvoiceDetailById")]
        public async Task<ActionResult<InvoiceDetailResponse>> GetInvoiceDetailById([FromQuery] GetInvoiceByIdRequest request)
        {
            var result = await _invoiceService.GetDetailByIdAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetInvoicesByFilter")]
        public async Task<ActionResult<InvoiceListResponse>> GetInvoicesByFilter(
            [FromQuery] EntityFilter<Invoice> filter,
            [FromQuery] EntitySort<Invoice> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _invoiceService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreateInvoice")]
        public async Task<ActionResult<InvoiceResponse>> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            var result = await _invoiceService.CreateAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice([FromBody] UpdateInvoiceRequest request)
        {
            await _invoiceService.UpdateAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteInvoice")]
        public async Task<IActionResult> DeleteInvoice([FromBody] DeleteInvoiceRequest request)
        {
            await _invoiceService.DeleteAsync(request);
            return Ok();
        }
    }
}
