namespace Backend_Boarding_house_management_system.Exceptions
{
    public abstract class AppException : Exception
    {
        public string ErrorCode { get; }

        protected AppException(string message, string errorCode = "") : base(message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? GetType().Name.Replace("Exception", "") : errorCode;
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

    public class ValidationException : AppException
    {
        public ValidationException(string message, string errorCode = "ValidationError") : base(message, errorCode) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message, string errorCode = "Unauthorized") : base(message, errorCode) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message, string errorCode = "Forbidden") : base(message, errorCode) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message, string errorCode = "Conflict") : base(message, errorCode) { }
    }

    public class InternalServerException : AppException
    {
        public InternalServerException(string message, string errorCode = "InternalServerError") : base(message, errorCode) { }
    }
}
