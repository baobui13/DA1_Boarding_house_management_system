namespace Backend_Boarding_house_management_system.DTOs.PropertyImage.Requests
{
    public class UpdatePropertyImageRequest
    {
        public string Id { get; set; } = null!;
        public bool? IsPrimary { get; set; }
    }
}
