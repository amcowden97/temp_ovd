using System.Collections.Generic;

namespace test_OVD_clientless.Models
{
    public class Workflow
    {
        public string name { get; set; }
	    public string path { get; set; }
        public ICollection<string> parameters { get; set; }
    }
}