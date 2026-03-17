using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Contract.Requests
{
    public class GetContractByIdRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
