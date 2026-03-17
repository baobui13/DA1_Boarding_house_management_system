using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.SearchHistory.Requests
{
    public class CreateSearchHistoryRequest
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string Filters { get; set; } = null!;
    }
}
