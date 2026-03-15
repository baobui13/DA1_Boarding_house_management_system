using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Amenity.Requests
{
    public class GetAmenitiesByFilterRequest : PagedRequest
    {
        public string? Name { get; set; }
    }
}
