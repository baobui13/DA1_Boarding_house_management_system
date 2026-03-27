using Backend_Boarding_house_management_system.DTOs.Payment.Requests;
using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetPaymentById")]
        public async Task<ActionResult<PaymentResponse>> GetPaymentById([FromQuery] GetPaymentByIdRequest request)
        {
            var result = await _paymentService.GetByIdAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetPaymentsByFilter")]
        public async Task<ActionResult<PaymentListResponse>> GetPaymentsByFilter(
            [FromQuery] EntityFilter<Payment> filter,
            [FromQuery] EntitySort<Payment> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _paymentService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPost("CreatePayment")]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var result = await _paymentService.CreateAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeletePayment")]
        public async Task<IActionResult> DeletePayment([FromBody] DeletePaymentRequest request)
        {
            await _paymentService.DeleteAsync(request);
            return Ok();
        }
    }
}
