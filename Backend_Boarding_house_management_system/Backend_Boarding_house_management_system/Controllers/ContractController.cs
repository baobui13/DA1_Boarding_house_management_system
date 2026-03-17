using Backend_Boarding_house_management_system.DTOs.Contract.Requests;
using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractController : ControllerBase
    {
        [HttpGet("GetContractById")]
        public async Task<ActionResult<ContractResponse>> GetContractById([FromQuery] GetContractByIdRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("GetContractsByFilter")]
        public async Task<ActionResult<ContractListResponse>> GetContractsByFilter([FromQuery] GetContractsByFilterRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CreateContract")]
        public async Task<ActionResult<ContractResponse>> CreateContract([FromBody] CreateContractRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("UpdateContract")]
        public async Task<IActionResult> UpdateContract([FromBody] UpdateContractRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("DeleteContract")]
        public async Task<IActionResult> DeleteContract([FromBody] DeleteContractRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
