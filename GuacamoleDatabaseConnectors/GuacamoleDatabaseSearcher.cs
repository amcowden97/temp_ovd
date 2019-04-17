using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace test_OVD_clientless.GuacamoleDatabaseConnectors
{
    public class GuacamoleDatabaseSearcher : IGuacSearcher
    {
        /// <summary>
        /// Searchs for the name of a specified group in the connection group table.
        /// </summary>
        /// <returns><c>true</c>, if group name was found, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        public bool SearchConnectionGroupName(string groupName, ref List<Exception> excepts)
        {
            const string queryString =
                "SELECT connection_group_name FROM guacamole_connection_group " +
                "WHERE connection_group_name=@input";

            Queue<string> queryResults = SearchQuery(queryString, groupName, ref excepts);
            return queryResults.Count != 0;
        }


        /// <summary>
        /// Searchs for the name of a specified group in the user group table.
        /// </summary>
        /// <returns><c>true</c>, if group name was found, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        public bool SearchUserGroupName(string groupName, ref List<Exception> excepts)
        {
            const string queryString =
                "SELECT name FROM guacamole_entity " +
                "WHERE name=@input AND type='USER_GROUP'";

            Queue<string> queryResults = SearchQuery(queryString, groupName, ref excepts);
            return queryResults.Count != 0;
        }

        /// <summary>
        /// Searchs for the dawgtag of the user.
        /// </summary>
        /// <returns><c>true</c>, if user name was found, <c>false</c> otherwise.</returns>
        /// <param name="dawgtag">Dawgtag.</param>
        public bool SearchUserName(string dawgtag, ref List<Exception> excepts)
        {
            const string queryString =
                "SELECT name FROM guacamole_entity " +
                "WHERE type='USER' AND name=@input";

            Queue<string> queryResults = SearchQuery(queryString, dawgtag, ref excepts);
            return queryResults.Count != 0;
        }


        /// <summary>
        /// Searchs for the name of the vm.
        /// </summary>
        /// <returns><c>true</c>, if vm name was found, <c>false</c> otherwise.</returns>
        /// <param name="vmName">Vm name.</param>
        public bool SearchVmName(string vmName, ref List<Exception> excepts)
        {
            const string queryString =
                "SELECT connection_name FROM guacamole_connection " +
                 "WHERE connection_name=@input";

            Queue<string> queryResults = SearchQuery(queryString, vmName, ref excepts);
            return queryResults.Count != 0;
        }


        /// <summary>
        /// Gets all group names.
        /// </summary>
        /// <returns>The all group names.</returns>
        /// <param name="excepts">Exceptions.</param>
        public Queue<string> GetAllGroupNames(ref List<Exception> excepts)
        {
            const string queryString =
                "SELECT connection_group_name FROM guacamole_connection_group";

            Queue<string> queryResults = SearchQuery(queryString, null, ref excepts);
            return queryResults;
        }


        /// <summary>
        /// Creates a list of every virtual machine connection.
        /// </summary>
        /// <returns>An ICollection<string> of all virtual machine names</returns>
        /// /// <param name="groupName">Group name.</param>
        public Queue<string> GetAllGroupVmNames(string groupName, ref List<Exception> excepts)
        {
            const string queryString =
                "SELECT connection_name FROM guacamole_connection, guacamole_connection_group " +
                "WHERE guacamole_connection.parent_id=guacamole_connection_group.connection_group_id " +
                "AND guacamole_connection_group.connection_group_name=@input";

            Queue<string> queryResults = SearchQuery(queryString, groupName, ref excepts);
            return queryResults;

        }


        /// <summary>
        /// General format for running a search query on the guacamole database
        /// </summary>
        /// <returns><c>true</c>, if field was found, <c>false</c> otherwise.</returns>
        /// <param name="queryString">Query string.</param>
        /// <param name="arg">Argument.</param>
        /// <param name="excepts">Exceptions.</param>
        private Queue<string> SearchQuery(string queryString, string arg, ref List<Exception> excepts)
        {
            //Stores the Values found from the database
            Queue<string> queryResults = new Queue<string>();
            try
            {
                using (GuacamoleDatabaseConnector gdbc = new GuacamoleDatabaseConnector(ref excepts))
                {
                    using (MySqlCommand query = new MySqlCommand(queryString, gdbc.getConnection()))
                    {
                        query.Prepare();

                        //Add the agruments given if found
                        if (arg != null)
                        {
                            query.Parameters.AddWithValue("@input", arg);
                        }

                        //Collect the query result column
                        using (MySqlDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                queryResults.Enqueue((string)reader[0]);
                            }
                        }
                        return queryResults;
                    }
                }
            }
            catch (Exception e)
            {
                excepts.Add(e);
                return queryResults;
            }
        }
    }
}
