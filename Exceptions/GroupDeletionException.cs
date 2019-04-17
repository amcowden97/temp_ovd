using System;
namespace test_OVD_clientless.Exceptions
{
    public class GroupDeletionException : Exception
    {
        public GroupDeletionException(string Message) : base(Message)
        {
        }
    }
}
