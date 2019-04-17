using System;
using System.Collections.Generic;
using test_OVD_clientless.Dtos;
using test_OVD_clientless.Controllers;
using test_OVD_clientless.Helpers;
using test_OVD_clientless.ScriptConnectors;

namespace test_OVD_clientless
{
    class Program
    {
        static void Main(string[] args)
        {
            NewGroupController gc = new NewGroupController();
            GroupForEditDto dto = new GroupForEditDto();
            Calculator calc = new Calculator();
            ScriptExecutor executor = new ScriptExecutor();

            List<Exception> excepts = new List<Exception>();
            IList<string> dawgtags = new List<string>();
            IList<string> removeDawgtags = new List<string>();

            dawgtags.Add("siu853401101");
            dawgtags.Add("siu853401102");

            removeDawgtags.Add("siu853401102");

            dto.Name = "Computer_Science_Class";
            dto.VMChoice = "Barkdoll-Guacamole-v1.4";
            dto.Memory = "1GHz @ 4xCPU, 4GB of Ram";
            dto.MinVms = 1;
            dto.MaxVms = 10;
            dto.NumHotspares = 1;
            dto.Protocol = "ssh";
            dto.Dawgtags = dawgtags;
            dto.RemoveDawgtags = removeDawgtags;

            gc.CreateConnections(dto, ref excepts);

            //Console.Write(calc.GetNextIp());

            //gc.CreateGroup("test", dto);

            //gc.DeleteGroup("test", "test_group_1");

            //gc.EditGroup("test", dto);
        }
    }
}
