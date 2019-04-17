using System;
using System.Collections.Generic;
using test_OVD_clientless.Dtos;
using test_OVD_clientless.Helpers;
using test_OVD_clientless.GuacamoleDatabaseConnectors;
using test_OVD_clientless.ScriptConnectors;

namespace test_OVD_clientless.Controllers
{
    public class GroupController
    {

        /// <summary>
        /// Deletes the specified connection group and user group.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="groupName">Group name.</param>
        public void DeleteGroup(string userId, string groupName)
        {
            List<Exception> excepts = new List<Exception>();
            GuacamoleDatabaseDeleter deleter = new GuacamoleDatabaseDeleter();
            deleter.DeleteUserGroup(groupName, ref excepts);
            deleter.DeleteConnectionGroup(groupName, ref excepts);

            if(excepts.Count == 0)
            {
                return;
            }
            else
            {
                Console.Write("Error");
                return;
            }
        }


        /// <summary>
        /// Gets all of the template names.
        /// </summary>
        /// <returns>The template names.</returns>
        public ICollection<string> GetTemplateNames()
        {
            ScriptExecutor executor = new ScriptExecutor();
            CloudmonkeyParser parser = new CloudmonkeyParser();

            string templateJson = executor.GetTemplateStats();
            return parser.ParseTemplateNames(templateJson);
        }


        /// <summary>
        /// Gets all of the service offering names.
        /// </summary>
        /// <returns>The service offering names.</returns>
        public ICollection<string> GetServiceOfferingNames()
        {
            ScriptExecutor executor = new ScriptExecutor();
            CloudmonkeyParser parser = new CloudmonkeyParser();

            string serviceOfferingJson = executor.GetServiceOfferingStats();
            return parser.ParseServiceOfferingNames(serviceOfferingJson);
        }


        /// <summary>
        /// Formats the given user inputs to ensure data consistancy when stored.
        /// </summary>
        /// <returns><c>true</c>, if the input was formated, <c>false</c> otherwise.</returns>
        /// <param name="groupForCreationDto">Group for creation dto.</param>
        /// <param name="excepts">Excepts.</param>
        protected bool FormatInput(GroupForCreationDto groupForCreationDto, ref List<Exception> excepts)
        {
            // Format User Text Input to be standardized to the following:
            //EX. test_group_1, ubuntu_16.04
            using (Formatter styler = new Formatter())
            {
                groupForCreationDto.Name = styler.FormatGroupName(groupForCreationDto.Name);
                groupForCreationDto.Protocol = styler.FormatName(groupForCreationDto.Protocol);

                for (int i = 0; i < groupForCreationDto.Dawgtags.Count; i++)
                {
                    groupForCreationDto.Dawgtags = styler.FormatDawgtagList(groupForCreationDto.Dawgtags);
                }
            }
            return true;
        }


        /// <summary>
        /// Validates the input for the list of dawgtags.
        /// </summary>
        /// <returns><c>true</c>, if the dawgtags were valid, <c>false</c> otherwise.</returns>
        /// <param name="groupForCreationDto">Group for creation dto.</param>
        /// <param name="excepts">Excepts.</param>
        protected bool ValidateInputForUsers(GroupForCreationDto groupForCreationDto, ref List<Exception> excepts)
        {
            using (Validator checker = new Validator())
            {
                //Check if the dawgtags are in the proper format
                foreach (string dawgtag in groupForCreationDto.Dawgtags)
                {
                    checker.ValidateDawgtag(dawgtag, ref excepts);
                }
            }
            return excepts.Count == 0;
        }


        /// <summary>
        /// Initializes the user by checking if they exist and creates a user
        /// if that user does not exist yet.
        /// </summary>
        /// <returns><c>true</c>, if user was initialized, <c>false</c> otherwise.</returns>
        /// <param name="dawgtag">Dawgtag.</param>
        /// <param name="excepts">Excepts.</param>
        protected bool InitializeUser(string dawgtag, ref List<Exception> excepts)
        {
            GuacamoleDatabaseInserter inserter = new GuacamoleDatabaseInserter();
            GuacamoleDatabaseSearcher searcher = new GuacamoleDatabaseSearcher();

            //Check if the user already exists
            if (!searcher.SearchUserName(dawgtag, ref excepts))
            {
                //Add the user if it was not found
                return inserter.InsertUser(dawgtag, ref excepts);
            }
            return true;
        }


        /// <summary>
        /// Adds the user to the user group.
        /// </summary>
        /// <returns><c>true</c>, if user was added to the user group, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="dawgtag">Dawgtag.</param>
        /// <param name="excepts">Excepts.</param>
        protected bool AddUserToUserGroup(string groupName, string dawgtag, ref List<Exception> excepts)
        {
            GuacamoleDatabaseInserter inserter = new GuacamoleDatabaseInserter();
            return inserter.InsertUserIntoUserGroup(groupName, dawgtag, ref excepts);
        }



        public bool CreateConnections(GroupForCreationDto groupForCreationDto, ref List<Exception> excepts)
        {
            Calculator calculator = new Calculator();
            CloudmonkeyParser jsonParser = new CloudmonkeyParser();
            GuacamoleDatabaseInserter inserter = new GuacamoleDatabaseInserter();
            ScriptExecutor executor = new ScriptExecutor();
            string connectionName, templateInfo, templateId, zoneInfo, zoneId, serviceOfferingInfo, serviceOfferingId;

            using (Formatter styler = new Formatter())
            {
                connectionName = styler.FormatVmName(groupForCreationDto.Name, ref excepts);
                if(connectionName == null)
                {
                    return false;
                }
            }

            //Get the virtual machine template information
            templateInfo = executor.GetTemplateStats();
            templateId = jsonParser.ParseTemplateId(templateInfo, groupForCreationDto.VMChoice);
            Console.WriteLine(templateId);

            //Get the virtual machine service offering info
            serviceOfferingInfo = executor.GetServiceOfferingStats();
            serviceOfferingId = jsonParser.ParseServiceOfferingId(serviceOfferingInfo, groupForCreationDto.Memory);
            Console.WriteLine(serviceOfferingId);

            //Get the zone information 
            zoneInfo = executor.GetZoneStats();
            zoneId = jsonParser.ParseZoneId(zoneInfo);
            Console.WriteLine(zoneId);

            for(int i = 0; i < groupForCreationDto.MinVms; i++)
            {
                //Deploy the new virtual machine
                string vmInfo = executor.DeployVirtualMachine(connectionName, templateId, serviceOfferingId, zoneId);
                string vmId = jsonParser.ParseVmId(vmInfo);
                Console.WriteLine(vmId);

                //Accquire a public ip address for the virtual machine
                string associatedIpInfo = executor.AccquireIp();
                string associatedIp = jsonParser.ParseAssociatedIpInfo(associatedIpInfo);
                string associatedIpId = jsonParser.ParseAssociatedIpId(associatedIpInfo);
                Console.WriteLine(associatedIp);
                Console.WriteLine(associatedIpId);

                //Setup the static nat for the accquired vm and ip
                Console.WriteLine(executor.SetStaticNat(vmId, associatedIpId));

                if (!inserter.InsertConnection(groupForCreationDto.Name, connectionName, associatedIp, 
                    groupForCreationDto.Protocol, ref excepts))
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Handles getting the error messages formatted.
        /// </summary>
        /// <returns>A string containing the error messages</returns>
        /// <param name="excepts">Exceptions.</param>
        protected String HandleErrors(List<Exception> excepts)
        {
            String exceptionMessage = "";
            foreach (Exception e in excepts)
            {
                Console.Error.Write(e.Message);
                exceptionMessage += e;
            }
            return exceptionMessage;
        }
    }
}
