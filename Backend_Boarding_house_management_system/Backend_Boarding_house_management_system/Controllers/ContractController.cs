using Backend_Boarding_house_management_system.DTOs.Contract.Requests;
using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
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
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetContractById")]
        public async Task<ActionResult<ContractResponse>> GetContractById([FromQuery] GetContractByIdRequest request)
        {
            var result = await _contractService.GetByIdAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Tenant,Admin")]
        [HttpGet("GetContractsByFilter")]
        public async Task<ActionResult<ContractListResponse>> GetContractsByFilter(
            [FromQuery] EntityFilter<Contract> filter,
            [FromQuery] EntitySort<Contract> sort,
            [FromQuery] EntityPage page)
        {
            var result = await _contractService.GetByFilterAsync(filter, sort, page);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPost("CreateContract")]
        public async Task<ActionResult<ContractResponse>> CreateContract([FromBody] CreateContractRequest request)
        {
            var result = await _contractService.CreateAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpPut("UpdateContract")]
        public async Task<IActionResult> UpdateContract([FromBody] UpdateContractRequest request)
        {
            await _contractService.UpdateAsync(request);
            return Ok();
        }

        [Authorize(Roles = "Landlord,Admin")]
        [HttpDelete("DeleteContract")]
        public async Task<IActionResult> DeleteContract([FromBody] DeleteContractRequest request)
        {
            await _contractService.DeleteAsync(request);
            return Ok();
        }
    }
}
