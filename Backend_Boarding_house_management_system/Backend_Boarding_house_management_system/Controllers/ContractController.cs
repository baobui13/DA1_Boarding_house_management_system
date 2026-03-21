using Backend_Boarding_house_management_system.DTOs.Contract.Requests;
using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractController : ControllerBase
    {
        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetContractById")]
        public async Task<ActionResult<ContractResponse>> GetContractById([FromQuery] GetContractByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetContractsByFilter")]
        public async Task<ActionResult<ContractListResponse>> GetContractsByFilter([FromQuery] GetContractsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreateContract")]
        public async Task<ActionResult<ContractResponse>> CreateContract([FromBody] CreateContractRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateContract")]
        public async Task<IActionResult> UpdateContract([FromBody] UpdateContractRequest request)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteContract")]
        public async Task<IActionResult> DeleteContract([FromBody] DeleteContractRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
