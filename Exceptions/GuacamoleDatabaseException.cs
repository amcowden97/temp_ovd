using System;
namespace test_OVD_clientless.Exceptions
{
    public class GuacamoleDatabaseException : Exception
    {
        public GuacamoleDatabaseException(string Message) : base(Message)
        {
        }
    }
}
