using System;

namespace ClientLourd.Services.RestService.Exceptions
{
    public class RestUnauthorizedException : RestException
    {
        public RestUnauthorizedException()
        {
        }

        public RestUnauthorizedException(string message)
            : base(message)
        {
        }

        public RestUnauthorizedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}