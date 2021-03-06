using System;

namespace ClientLourd.Services.RestService.Exceptions
{
    public class RestNotFoundException : RestException
    {
        public RestNotFoundException()
        {
        }

        public RestNotFoundException(string message)
            : base(message)
        {
        }

        public RestNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}