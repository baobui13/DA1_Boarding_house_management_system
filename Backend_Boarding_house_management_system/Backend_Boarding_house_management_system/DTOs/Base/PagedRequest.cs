namespace Backend_Boarding_house_management_system.DTOs.Base
{
    public class PagedRequest
    {
        public string? SortBy { get; set; } = "CreatedAt"; // Default sorting field

        public bool IsDescending { get; set; } = true; // Default sorting order

        public int PageNumber { get; set; } = 1; // Default page number

        public int PageSize { get; set; } = 10; // Default page size
    }
}