using System.Collections.Generic;

namespace test_OVD_clientless.Models
{
    public class WorkflowResult
    {
        public int exitCode { get; set; }
        public string supplierId { get; set; }
        public ICollection<string> messages { get; set; }
    }
}
