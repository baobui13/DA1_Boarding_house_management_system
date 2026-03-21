using Backend_Boarding_house_management_system.DTOs.Payment.Requests;
using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetPaymentById")]
        public async Task<ActionResult<PaymentResponse>> GetPaymentById([FromQuery] GetPaymentByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetPaymentsByFilter")]
        public async Task<ActionResult<PaymentListResponse>> GetPaymentsByFilter([FromQuery] GetPaymentsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Tenant,Admin")]
        [HttpPost("CreatePayment")]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeletePayment")]
        public async Task<IActionResult> DeletePayment([FromBody] DeletePaymentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
