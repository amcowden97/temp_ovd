using System;
using System.Xml;

namespace test_OVD_clientless.Helpers
{
    public class Calculator
    {
        /// <summary>
        /// Generates a random identifier.
        /// </summary>
        /// <returns>The id.</returns>
        public string GenerateId()
        {
            return String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
        }
    }
}
