using System;
using System.Collections.Generic;

namespace test_OVD_clientless.Dtos
{
    public class GroupForCreationDto
    {
        public String Name { get; set; }
        public String VMChoice { get; set; }
        public string Memory { get; set; }
        public String Protocol { get; set; }
        public int MaxVms { get; set; }
        public int MinVms { get; set; }
        public int NumHotspares { get; set; }
        public IList<String> Dawgtags { get; set; }
    }
}
