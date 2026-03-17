using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Contract.Requests
{
    public class DeleteContractRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
