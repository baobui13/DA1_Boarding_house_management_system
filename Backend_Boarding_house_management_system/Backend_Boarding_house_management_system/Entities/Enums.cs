namespace Backend_Boarding_house_management_system.Entities
{
    public enum UserRole
    {
        Admin,
        Landlord,
        Tenant
    }

    public enum ModerationStatusEnum
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum AvailabilityStatusEnum
    {
        Available = 0,
        Rented = 1,
        Maintenance = 2
    }

    public enum PropertyStatus
    {
        PendingApproval, 
        Approved,
        Rejected, 
        Available,
        Rented,
        Repairing,
        NearExpiry
    }

    public enum AmenityStatus
    {
        Working,
        Broken,
        Repairing
    }

    public enum ContractStatus
    {
        Draft,
        Active,
        NearExpiry,
        Expired,
        Terminated,
        Cancelled
    }

    public enum InvoiceStatus
    {
        Pending,
        Partial,
        Paid
    }

    public enum PaymentMethod
    {
        Cash,
        BankTransfer,
        Online
    }

    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Cancelled
    }

    public enum NotificationType
    {
        Invoice,
        Appointment,
        Contract,
        Message,
        System,
        Rating
    }

    public enum DocumentType
    {
        IDCard,
        ResidencePermit,
        Other
    }
    
    public enum RatingAttitude
    {
        Positive,
        Negative,
        Neutral
    }
    
    public enum ComplaintStatus
    {
        Pending,
        Processing,
        Resolved
    }
    
    public enum ComplaintRelatedType
    {
        Invoice,
        Contract,
        Property
    }
}
