﻿namespace CryptoExchange.Net.Objects
{
    public abstract class Error
    {
        public int Code { get; set; }
        public string Message { get; set; }

        protected Error(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Code}: {Message}";
        }
    }

    public class CantConnectError : Error
    {
        public CantConnectError() : base(1, "Can't connect to the server") { }
    }

    public class NoApiCredentialsError : Error
    {
        public NoApiCredentialsError() : base(2, "No credentials provided for private endpoint") { }
    }

    public class ServerError: Error
    {
        public ServerError(string message) : base(3, "Server error: " + message) { }

        public ServerError(int code, string message) : base(code, message)
        {
        }
    }

    public class WebError : Error
    {
        public WebError(string message) : base(4, "Web error: " + message) { }
    }

    public class DeserializeError : Error
    {
        public DeserializeError(string message) : base(5, "Error deserializing data: " + message) { }
    }

    public class UnknownError : Error
    {
        public UnknownError(string message) : base(6, "Unknown error occured " + message) { }
    }

    public class ArgumentError : Error
    {
        public ArgumentError(string message) : base(7, "Invalid parameter: " + message) { }
    }

    public class RateLimitError: Error
    {
        public RateLimitError(string message) : base(8, "Rate limit exceeded: " + message) { }
    }
}
