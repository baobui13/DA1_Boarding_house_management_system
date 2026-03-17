using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.ViewHistory.Requests
{
    public class DeleteViewHistoryRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
