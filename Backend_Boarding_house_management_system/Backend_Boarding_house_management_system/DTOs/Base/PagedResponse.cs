namespace Backend_Boarding_house_management_system.DTOs.Base
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();

        public int TotalCount { get; set; } // Total number of items matching the filter

        public int PageNumber { get; set; } // Current page number

        public int PageSize { get; set; } // Number of items per page

        public PagedResponse()
        {
        }

        public PagedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}