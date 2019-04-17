using System;
using System.Collections.Generic;
using test_OVD_clientless.Dtos;
using test_OVD_clientless.GuacamoleDatabaseConnectors;
using test_OVD_clientless.Helpers;

namespace test_OVD_clientless.Controllers
{
    public class EditGroupController : GroupController
    {
        public void EditGroup(string userId, GroupForEditDto groupForEditDto)
        {
            //Method Level Variable Declarations
            List<Exception> excepts = new List<Exception>();

            //Format the given input
            if (!FormatInput(groupForEditDto, ref excepts))
            {
                var message = HandleErrors(excepts);
                return; //BadRequest(message);
            }

            //Validate group input parameters
            if (!ValidateInputForExistingGroup(groupForEditDto, ref excepts))
            {
                var message = HandleErrors(excepts);
                return; //BadRequest(message);
            }

            //Validate user input parameters
            if (!ValidateInputForUsers(groupForEditDto, ref excepts))
            {
                var message = HandleErrors(excepts);
                return; //BadRequest(message);
            }

            //Create users if they do not exist in the system and add them to the user group
            foreach (string dawgtag in groupForEditDto.Dawgtags)
            {
                //Verify a user exists and create them if they do not
                if (InitializeUser(dawgtag, ref excepts))
                {
                    AddUserToUserGroup(groupForEditDto.Name, dawgtag, ref excepts);
                }
            }

            //Delete the specified users from the group
            foreach (string dawgtag in groupForEditDto.RemoveDawgtags)
            {
                RemoveUserFromGroup(groupForEditDto.Name, dawgtag, ref excepts);
            }
        }


        /// <summary>
        /// Validates the user input for group parameters.
        /// </summary>
        /// <returns><c>true</c>, if input parameters for the group is valid, <c>false</c> otherwise.</returns>
        /// <param name="groupForEditDto">Group for creation dto.</param>
        /// <param name="excepts">Excepts.</param>
        protected bool ValidateInputForExistingGroup(GroupForEditDto groupForEditDto, ref List<Exception> excepts)
        {
            using (Validator checker = new Validator())
            {
                //Check if the group inupt parameters are valid
                checker.ValidateExistingGroupName(groupForEditDto.Name, ref excepts);
                checker.ValidateConnectionType(groupForEditDto.VMChoice, ref excepts);
                checker.ValidateMin(groupForEditDto.MinVms, ref excepts);
                checker.ValidateMax(groupForEditDto.MaxVms, ref excepts);
                checker.ValidateMinMax(groupForEditDto.MinVms, groupForEditDto.MaxVms, ref excepts);
                checker.ValidateHotspares(groupForEditDto.NumHotspares, ref excepts);
            }
            return excepts.Count == 0;
        }


        /// <summary>
        /// Deletes the user from the given user group.
        /// </summary>
        /// <returns><c>true</c>, if user was deleted from the user group, <c>false</c> otherwise.</returns>
        /// <param name="dawgtag">Dawgtag.</param>
        /// <param name="excepts">Excepts.</param>
        private bool RemoveUserFromGroup(string groupName, string dawgtag, ref List<Exception> excepts)
        {
            GuacamoleDatabaseDeleter deleter = new GuacamoleDatabaseDeleter();
            return deleter.DeleteUserFromUserGroup(groupName, dawgtag, ref excepts);
        }
    }
}
