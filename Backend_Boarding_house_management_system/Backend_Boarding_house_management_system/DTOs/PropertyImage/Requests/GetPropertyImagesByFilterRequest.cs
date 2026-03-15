namespace Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests
{
    public class GetPropertyImagesByFilterRequest
    {
        public string? PropertyId { get; set; }
        public bool? IsPrimary { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
