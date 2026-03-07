namespace Backend_Boarding_house_management_system.DTOs.Property.Requests
{
    public class GetPropertiesByFilterRequest : Backend_Boarding_house_management_system.DTOs.Base.PagedRequest
    {
        public string? LandlordId { get; set; }
        public string? AreaId { get; set; }
        public string? PropertyName { get; set; }
        public string? Address { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Status { get; set; }
        public decimal? MinSize { get; set; }
        public decimal? MaxSize { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
    }
}
