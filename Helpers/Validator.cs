using System;
using System.IO;
using System.Text.RegularExpressions;
using test_OVD_clientless.GuacamoleDatabaseConnectors;
using test_OVD_clientless.Exceptions;
using System.Collections.Generic;
using System.Xml;

namespace test_OVD_clientless.Helpers
{
    public class Validator : IDisposable
    {
        private bool isDisposed = false;


        /// <summary>
        /// Releases all resource used by the <see cref="T:test_OVD_clientless.Helpers.Validator"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:test_OVD_clientless.Helpers.Validator"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:test_OVD_clientless.Helpers.Validator"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:test_OVD_clientless.Helpers.Validator"/> so the garbage collector can reclaim the memory that
        /// the <see cref="T:test_OVD_clientless.Helpers.Validator"/> was occupying.</remarks>
        public void Dispose()
        {
            ReleaseResources(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the managed and unmanaged resources.
        /// </summary>
        /// <param name="isFromDispose">If set to <c>true</c> is from dispose.</param>
        protected void ReleaseResources(bool isFromDispose)
        {
            if (!isDisposed)
            {
                if (isFromDispose)
                {
                    // TODO: Release managed resources here
                }
                //TODO: Release unmanaged resources here
            }
            isDisposed = true;
        }


        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:test_OVD_clientless.Helpers.Validator"/> is reclaimed by garbage collection.
        /// </summary>
        ~Validator()
        {
            ReleaseResources(false);
        }


        /*******************************************************************************
         *------------------------Primary Validator Methods----------------------------*
         ******************************************************************************/
        /// <summary>
        /// Validates the name of the group by checking if the given name exists within
        /// the guacamole database.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        public bool ValidateNewGroupName(string groupName, ref List<Exception> exceptions)
        {
            string execptMessage = $"The given group name {groupName} already " +
                "exists. Please choose another name or edit the existing group.";

            GuacamoleDatabaseSearcher searcher = new GuacamoleDatabaseSearcher();
            if (searcher.SearchConnectionGroupName(groupName, ref exceptions))
            {
                exceptions.Add(new ValidationException(execptMessage));
                return false;
            }
            if (searcher.SearchUserGroupName(groupName, ref exceptions))
            {
                exceptions.Add(new ValidationException(execptMessage));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Validates the existing name of a group.
        /// </summary>
        /// <returns><c>true</c>, if new group name was validated, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="exceptions">Exceptions.</param>
        public bool ValidateExistingGroupName(string groupName, ref List<Exception> exceptions)
        {
            string execptMessage = $"The given group name {groupName} does not exist.";

            GuacamoleDatabaseSearcher searcher = new GuacamoleDatabaseSearcher();
            if (!searcher.SearchConnectionGroupName(groupName, ref exceptions))
            {
                exceptions.Add(new ValidationException(execptMessage));
                return false;
            }
            if (!searcher.SearchUserGroupName(groupName, ref exceptions))
            {
                exceptions.Add(new ValidationException(execptMessage));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Validates if vm type exists within the given configuration file.
        /// </summary>
        /// <param name="vmChoice">Vm choice.</param>
        public bool ValidateConnectionType(string vmChoice, ref List<Exception> exceptions)
        {
            const string CONFIG_FILE_LOC = "./ConfigurationFiles/ConnectionTypes.xml";
            string exceptMessage = $"The given connection type {vmChoice} is not " +
                "an acceptable connection type.";

            //Read in the xml connection type configuration file
            XmlDocument config = new XmlDocument();
            config.Load(CONFIG_FILE_LOC);

            XmlNodeList nodes = config.SelectNodes("//connection[@name='" + vmChoice + "']");
            if(nodes.Count == 0)
            {
                exceptions.Add(new ValidationException(exceptMessage));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Validates if the protocol is supported for the given connection type.
        /// </summary>
        /// <returns><c>true</c>, if protocol was validated, <c>false</c> otherwise.</returns>
        /// <param name="vmChoice">Vm choice.</param>
        /// <param name="protocol">Protocol.</param>
        /// <param name="exceptions">Exceptions.</param>
        public bool ValidateProtocol(string vmChoice, string protocol, ref List<Exception> exceptions)
        {
            const string CONFIG_FILE_LOC = "./ConfigurationFiles/ConnectionTypes.xml";
            string exceptMessage = $"The given protocol {protocol} is not available for " +
                "the conneciton type {vmChoice}.";

            //Read in the xml connection type configuration file
            XmlDocument config = new XmlDocument();
            config.Load(CONFIG_FILE_LOC);

            XmlNodeList nodes = config.SelectNodes("//connection[@name='" + vmChoice + "']");
            foreach (XmlNode node in nodes)
            {
                foreach(XmlNode childNode in node.ChildNodes)
                {
                    if ("supported-protocol".Equals(childNode.Name))
                    {
                        return (protocol.Equals(childNode.InnerText));
                    }
                }
            }
            exceptions.Add(new ValidationException(exceptMessage));
            return false;
        }


        /// <summary>
        /// Validates the minimum vm number provided.
        /// </summary>
        /// <param name="min">Minimum.</param>
        public bool ValidateMin(int min, ref List<Exception> exceptions)
        {
            const string exceptMessage = "The minimum number of connections given " +
                "must be greater than or equal to zero.";

            if (!CheckPositiveInputNumber(min))
            {
                exceptions.Add(new ValidationException(exceptMessage));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Validates the maximum vm number provided.
        /// </summary>
        /// <param name="max">Maximum.</param>
        public bool ValidateMax(int max, ref List<Exception> exceptions)
        {
            const string exceptMessage = "The maximum number of connections given " +
                "must be greater than or equal to zero.";
            if (!CheckPositiveInputNumber(max))
            {
                exceptions.Add(new ValidationException(exceptMessage));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Validates that the minimum vm value is less than or equal to the maximum.
        /// </summary>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public bool ValidateMinMax(int min, int max, ref List<Exception> exceptions)
        {
            const string exceptMessage = "The minimum number of connections must be " +
                "less than the maximum number of connections.";
            if (min > max)
            {
                exceptions.Add(new ValidationException(exceptMessage));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Validates the hotspare number provided.
        /// </summary>
        /// <param name="hotspareNumber">Hotspare number.</param>
        public bool ValidateHotspares(int hotspareNumber, ref List<Exception> exceptions)
        {
            const string exceptMessage = "The number of hotspares provided must be " +
                "greater than or equal to zero.";

            if (!CheckPositiveInputNumber(hotspareNumber))
            {
                exceptions.Add(new ValidationException(exceptMessage));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Ensures that the dawgtag given is in the proper format.
        /// </summary>
        /// <param name="dawgtag">Dawgtag.</param>
        public bool ValidateDawgtag(string dawgtag, ref List<Exception> exceptions)
        {
            string exceptMessage = $"The given dawgtag {dawgtag} is not in the proper " +
                "format.";

            //Ensure dawg tag is in the proper format
            Regex regex = new Regex(@"siu85\d{7}\z", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!regex.Match(dawgtag).Success)
            {
                exceptions.Add(new ValidationException(exceptMessage));
                return false;
            }
            return true;
        }


        /// <summary> 
        /// Checks if the provided integer is greater than or equal to zero.
        /// This is used for validating user integer input. 
        /// </summary>
        /// <returns><c>true</c>, if input number was validated, <c>false</c> otherwise.</returns>
        /// <param name="number">The integer to validate.</param>
        private bool CheckPositiveInputNumber(int number)
        {
            return (number >= 0);
        }
    }
}
