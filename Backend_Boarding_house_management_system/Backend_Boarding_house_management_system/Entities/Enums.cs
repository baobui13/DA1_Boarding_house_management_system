namespace Backend_Boarding_house_management_system.Entities
{
    public enum UserRole
    {
        Admin,
        Landlord,
        Tenant
    }

    public enum PropertyStatus
    {
        PendingApproval, 
        Approved,
        Rejected, 
        Available,
        Rented,
        Unavailable
    }

    public enum AmenityStatus
    {
        Working,
        Broken,
        Repairing
    }

    public enum ContractStatus
    {
        Active,
        Expired,
        Terminated
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
        System
    }

    public enum DocumentType
    {
        IDCard,
        ResidencePermit,
        Other
    }
}
