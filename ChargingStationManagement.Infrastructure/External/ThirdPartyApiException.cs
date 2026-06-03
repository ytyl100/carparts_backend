// ChargingStationManagement.Infrastructure/External/ThirdPartyApiException.cs
using System;

namespace ChargingStationManagement.Infrastructure.External
{
    public class ThirdPartyApiException : Exception
    {
        public int ErrorCode { get; }

        public ThirdPartyApiException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ThirdPartyApiException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}