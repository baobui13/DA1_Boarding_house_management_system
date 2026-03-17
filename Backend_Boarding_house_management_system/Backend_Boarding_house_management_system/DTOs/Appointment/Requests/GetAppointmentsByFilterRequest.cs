using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Appointment.Requests
{
    public class GetAppointmentsByFilterRequest : PagedRequest
    {
        public string? RoomId { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
