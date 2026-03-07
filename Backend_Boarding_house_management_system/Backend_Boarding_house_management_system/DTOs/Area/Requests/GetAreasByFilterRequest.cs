using System;
namespace Backend_Boarding_house_management_system.DTOs.Area.Requests
{
    public class GetAreasByFilterRequest : Backend_Boarding_house_management_system.DTOs.Base.PagedRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? LandlordId { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
    }
}
