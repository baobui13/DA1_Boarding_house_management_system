using System.ComponentModel.DataAnnotations;
namespace Backend_Boarding_house_management_system.DTOs.Area.Requests
{
    public class DeleteAreaRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
