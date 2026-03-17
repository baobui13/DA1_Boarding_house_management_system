using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests
{
    public class DeleteSearchHistoryRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
