namespace Backend_Boarding_house_management_system.Exceptions
{
    public abstract class AppException : Exception
    {
        public string ErrorCode { get; }
        protected AppException(string message, string errorCode = null) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message, string errorCode = "NotFound") : base(message, errorCode) { }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message, string errorCode = "BadRequest") : base(message, errorCode) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message, string errorCode = "Unauthorized") : base(message, errorCode) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message, string errorCode = "Conflict") : base(message, errorCode) { }
    }
}
