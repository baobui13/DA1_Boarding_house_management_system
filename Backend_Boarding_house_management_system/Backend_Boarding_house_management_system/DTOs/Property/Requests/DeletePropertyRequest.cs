using System.ComponentModel.DataAnnotations;
namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class DeletePropertyRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
