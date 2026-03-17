using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Message.Requests
{
    public class GetMessagesByFilterRequest : PagedRequest
    {
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? RoomId { get; set; }
        public string? ContractId { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
    }
}
