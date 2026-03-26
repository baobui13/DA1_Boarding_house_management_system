using Backend_Boarding_house_management_system.DTOs.Contract.Requests;
using Backend_Boarding_house_management_system.DTOs.Contract.Responses;
using Backend_Boarding_house_management_system.Entities;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IContractService
    {
        Task<ContractResponse> GetByIdAsync(GetContractByIdRequest request);
        Task<ContractListResponse> GetByFilterAsync(
            EntityFilter<Contract> filter,
            EntitySort<Contract> sort,
            EntityPage page);
        Task<ContractResponse> CreateAsync(CreateContractRequest request);
        Task UpdateAsync(UpdateContractRequest request);
        Task DeleteAsync(DeleteContractRequest request);
    }
}
