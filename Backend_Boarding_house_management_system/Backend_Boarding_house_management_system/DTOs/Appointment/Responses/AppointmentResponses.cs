using Backend_Boarding_house_management_system.DTOs.Base;

namespace Backend_Boarding_house_management_system.DTOs.Appointment.Responses
{
    public class AppointmentResponse
    {
        public string Id { get; set; } = null!;
        public string PropertyId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AppointmentListResponse : PagedResponse<AppointmentResponse>
    {
    }
}
