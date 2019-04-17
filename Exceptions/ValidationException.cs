using System;
namespace test_OVD_clientless.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string Message) : base(Message)
        {
        }
    }
}
