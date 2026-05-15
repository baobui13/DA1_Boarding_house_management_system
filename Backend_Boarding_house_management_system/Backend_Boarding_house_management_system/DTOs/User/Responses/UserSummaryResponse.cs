namespace Backend_Boarding_house_management_system.DTOs.User.Responses
{
    public class UserSummaryResponse
    {
        public int TotalUsers { get; set; }

        public int TotalActive { get; set; }

        public int TotalLocked { get; set; }

        public int TotalLandlords { get; set; }
    }
}
