using System;
using System.Collections.Generic;

namespace test_OVD_clientless.Dtos
{
    public class GroupForEditDto : GroupForCreationDto
    {
        public IList<String> RemoveDawgtags { get; set; }
    }
}
