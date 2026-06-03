namespace ChargingStationManagement.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string ErrorCode { get; }
        public int HttpStatusCode { get; }

        protected DomainException(string message, string errorCode, int httpStatusCode = 400)
            : base(message)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }
    }

    public class ValidationException : DomainException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(string message, Dictionary<string, string[]> errors)
            : base(message, "VALIDATION_ERROR", 400)
        {
            Errors = errors;
        }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, string entityId)
            : base($"{entityName} with ID '{entityId}' was not found.", "NOT_FOUND", 404)
        {
        }

        public NotFoundException(string message)
            : base(message, "NOT_FOUND", 404)
        {
        }
    }

    public class AlreadyExistsException : DomainException
    {
        public AlreadyExistsException(string entityName, string entityId)
            : base($"{entityName} with ID '{entityId}' already exists.", "ALREADY_EXISTS", 409)
        {
        }
    }

    public class InvalidOperationException : DomainException
    {
        public InvalidOperationException(string message)
            : base(message, "INVALID_OPERATION", 400)
        {
        }
    }

    public class InsufficientBalanceException : DomainException
    {
        public decimal CurrentBalance { get; }
        public decimal RequiredAmount { get; }

        public InsufficientBalanceException(decimal currentBalance, decimal requiredAmount)
            : base($"Insufficient balance. Current: {currentBalance}, Required: {requiredAmount}",
                  "INSUFFICIENT_BALANCE", 400)
        {
            CurrentBalance = currentBalance;
            RequiredAmount = requiredAmount;
        }
    }

    public class ChargingSessionException : DomainException
    {
        public ChargingSessionException(string message)
            : base(message, "CHARGING_SESSION_ERROR", 400)
        {
        }
    }

    public class AuthenticationException : DomainException
    {
        public AuthenticationException(string message)
            : base(message, "AUTHENTICATION_ERROR", 401)
        {
        }
    }

    public class AuthorizationException : DomainException
    {
        public AuthorizationException(string message)
            : base(message, "AUTHORIZATION_ERROR", 403)
        {
        }
    }

    public class ThirdPartyApiException : DomainException
    {
        public int ApiErrorCode { get; }
        public string ApiErrorMessage { get; }

        public ThirdPartyApiException(string message, int apiErrorCode, string apiErrorMessage)
            : base(message, "THIRD_PARTY_API_ERROR", 502)
        {
            ApiErrorCode = apiErrorCode;
            ApiErrorMessage = apiErrorMessage;
        }
    }

    public class PaymentException : DomainException
    {
        public PaymentException(string message, string errorCode = "PAYMENT_ERROR")
            : base(message, errorCode, 400)
        {
        }
    }

    public class DatabaseException : DomainException
    {
        public DatabaseException(string message)
            : base(message, "DATABASE_ERROR", 500)
        {
        }
    }

    public class NetworkException : DomainException
    {
        public NetworkException(string message)
            : base(message, "NETWORK_ERROR", 503)
        {
        }
    }

    public class RateLimitException : DomainException
    {
        public TimeSpan RetryAfter { get; }

        public RateLimitException(string message, TimeSpan retryAfter)
            : base(message, "RATE_LIMIT_EXCEEDED", 429)
        {
            RetryAfter = retryAfter;
        }
    }
}