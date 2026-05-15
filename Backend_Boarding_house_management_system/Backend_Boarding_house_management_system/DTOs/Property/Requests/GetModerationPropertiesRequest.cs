using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class GetModerationPropertiesRequest : PagedRequest
    {
        public string Status { get; set; } = "Pending";
    }
}
