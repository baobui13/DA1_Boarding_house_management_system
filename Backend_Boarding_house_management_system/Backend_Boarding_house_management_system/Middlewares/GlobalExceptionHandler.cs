using Backend_Boarding_house_management_system.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Da xay ra loi: {Message}", exception.Message);

            var (statusCode, title, errorCode) = exception switch
            {
                ValidationException e => (StatusCodes.Status400BadRequest, "Du lieu khong hop le (Validation Error)", e.ErrorCode),
                BadRequestException e => (StatusCodes.Status400BadRequest, "Yeu cau khong hop le (Bad Request)", e.ErrorCode),
                UnauthorizedException e => (StatusCodes.Status401Unauthorized, "Khong the xac thuc (Unauthorized)", e.ErrorCode),
                ForbiddenException e => (StatusCodes.Status403Forbidden, "Khong duoc phep truy cap (Forbidden)", e.ErrorCode),
                NotFoundException e => (StatusCodes.Status404NotFound, "Khong tim thay du lieu (Not Found)", e.ErrorCode),
                ConflictException e => (StatusCodes.Status409Conflict, "Xung dot du lieu (Conflict)", e.ErrorCode),
                InternalServerException e => (StatusCodes.Status500InternalServerError, "Loi may chu noi bo (Internal Server Error)", e.ErrorCode),
                _ => (StatusCodes.Status500InternalServerError, "Loi may chu noi bo (Internal Server Error)", null)
            };

            var isAppException = exception is AppException;
            var detailMessage = isAppException || _env.IsDevelopment()
                ? exception.Message
                : "Da xay ra loi he thong, vui long thu lai sau.";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detailMessage,
                Instance = httpContext.Request.Path
            };

            if (!string.IsNullOrWhiteSpace(errorCode))
            {
                problemDetails.Extensions["errorCode"] = errorCode;
            }

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
