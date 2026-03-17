using Backend_Boarding_house_management_system.DTOs.Payment.Requests;
using Backend_Boarding_house_management_system.DTOs.Payment.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        [HttpGet("GetPaymentById")]
        public async Task<ActionResult<PaymentResponse>> GetPaymentById([FromQuery] GetPaymentByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetPaymentsByFilter")]
        public async Task<ActionResult<PaymentListResponse>> GetPaymentsByFilter([FromQuery] GetPaymentsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreatePayment")]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeletePayment")]
        public async Task<IActionResult> DeletePayment([FromBody] DeletePaymentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
