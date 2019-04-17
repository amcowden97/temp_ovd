using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using test_OVD_clientless.Exceptions;

namespace test_OVD_clientless.GuacamoleDatabaseConnectors
{
    public class GuacamoleDatabaseInserter : IGuacInserter
    {
   
        /// <summary>
        /// Inserts the given parameters into the guacamole database as a new
        /// connection group.
        /// </summary>
        /// <returns><c>true</c>, if connection group was inserted, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="max">Max VMs.</param>
        /// <param name="excepts">Exceptions.</param>
        public bool InsertConnectionGroup(string groupName, int max, ref List<Exception> excepts)
        {
            const string MAX_USER_CONNECTIONS = "1";
            const string CONNECTION_GROUP_TYPE = "BALANCING";
            const string queryString =
                "INSERT INTO guacamole_connection_group (connection_group_name, max_connections, max_connections_per_user, type) " +
                "VALUES (@groupname, @maxconnections, @maxuserconnections, @type)";

            Queue<string> argNames = new Queue<string>();
            argNames.Enqueue("@groupname");
            argNames.Enqueue("@maxconnections");
            argNames.Enqueue("@maxuserconnections");
            argNames.Enqueue("@type");

            try
            {
                Queue<string> args = new Queue<string>();
                args.Enqueue(groupName);
                args.Enqueue(max.ToString());
                args.Enqueue(MAX_USER_CONNECTIONS);
                args.Enqueue(CONNECTION_GROUP_TYPE);
                return InsertQuery(queryString, argNames, args, ref excepts);
            }
            catch(Exception e)
            {
                excepts.Add(e);
                return false;
            }
        }


        /// <summary>
        /// Inserts the new user group.
        /// </summary>
        /// <returns><c>true</c>, if the newuser group was inserted, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="excepts">Exceptions.</param>
        public bool InsertUserGroup(string groupName, ref List<Exception> excepts)
        {
            const string entityQueryString =
                "INSERT INTO guacamole_entity (name, type) " +
                "VALUES (@groupname, 'USER_GROUP')";

            const string entityIdQueryString =
                "(SELECT entity_id FROM guacamole_entity " +
                "WHERE name = @groupname AND type = 'USER_GROUP')";

            const string groupQueryString =
                "INSERT INTO guacamole_user_group (entity_id) " +
                "VALUES (" + entityIdQueryString + ")";

            Queue<string> argNames = new Queue<string>();
            argNames.Enqueue("@groupname");

            Queue<string> args = new Queue<string>();
            args.Enqueue(groupName);

            //Insert the usergroup into the entity table
            if (InsertQuery(entityQueryString, argNames, args, ref excepts))
            {
                //Insert the usergroup into the user groups table
                return InsertQuery(groupQueryString, argNames, args, ref excepts);
            }
            return false;
        }


        /// <summary>
        /// Inserts a new user into the guacamole entity table and the guacamole 
        /// user table. This method assumes that authentication is handled external
        /// of the guacamole database. This is why the placeholder of INVALID is used
        /// for the password hash field as required by the guacamole database.
        /// </summary>
        /// <returns><c>true</c>, if user was inserted, <c>false</c> otherwise.</returns>
        /// <param name="dawgtag">Dawgtag.</param>
        public bool InsertUser(string dawgtag, ref List<Exception> excepts)
        {
            const string entityQueryString =
                "INSERT INTO guacamole_entity (name, type) " +
                "VALUES (@username, 'USER')";

            const string entityIdQueryString =
                "(SELECT entity_id FROM guacamole_entity " +
                "WHERE name = @username AND type = 'USER')";

            const string userQueryString =
                "INSERT INTO guacamole_user (entity_id, password_hash, password_date) " +
                "VALUES (" + entityIdQueryString + ", 'INVALID', CURRENT_TIMESTAMP)";
                
            Queue<string> argNames = new Queue<string>();
            argNames.Enqueue("@username");

            Queue<string> args = new Queue<string>();
            args.Enqueue(dawgtag);

            //Insert the user into the entity table
            if (InsertQuery(entityQueryString, argNames, args, ref excepts))
            {
                //Insert user and fake hash into the user users table
                return InsertQuery(userQueryString, argNames, args, ref excepts);
            }
            return false;
        }


        /// <summary>
        /// Inserts the user into the user group.
        /// </summary>
        /// <returns><c>true</c>, if user was inserted into the user group<c>false</c> otherwise.</returns>
        /// <param name="dawgtag">Dawgtag.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="excepts">Exceptions.</param>
        public bool InsertUserIntoUserGroup(string groupName, string dawgtag, ref List<Exception> excepts)
        {
            const string groupIdQueryString =
                "(SELECT user_group_id FROM guacamole_user_group, guacamole_entity " +
                "WHERE name = @groupname AND type = 'USER_GROUP' " +
                "AND guacamole_user_group.entity_id=guacamole_entity.entity_id)";

            const string userIdQueryString =
                "(SELECT entity_id FROM guacamole_entity " +
                "WHERE name = @username AND type = 'USER')";

            const string memberQueryString =
                "INSERT INTO guacamole_user_group_member (user_group_id, member_entity_id) " +
                "VALUES (" + groupIdQueryString + "," + userIdQueryString + ")";

            Queue<string> argNames = new Queue<string>();
            argNames.Enqueue("@groupname");
            argNames.Enqueue("@username");

            Queue<string> args = new Queue<string>();
            args.Enqueue(groupName);
            args.Enqueue(dawgtag);

            //Insert the user into the entity table
            return InsertQuery(memberQueryString, argNames, args, ref excepts);
        }


        /// <summary>
        /// Inserts the connection group into user group.
        /// </summary>
        /// <returns><c>true</c>, if connection group into user group was inserted, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="excepts">Excepts.</param>
        public bool InsertConnectionGroupIntoUserGroup(string groupName, ref List<Exception> excepts)
        {
            const string connectionGroupIdQueryString =
                "(SELECT connection_group_id FROM guacamole_connection_group " +
                "WHERE connection_group_name = @groupname)";

            const string userGroupIdQueryString =
                "(SELECT entity_id FROM guacamole_entity " +
                "WHERE name = @groupname AND type = 'USER_GROUP')";

            const string memberQueryString =
                "INSERT INTO guacamole_connection_group_permission (entity_id, connection_group_id, permission) " +
                "VALUES (" + userGroupIdQueryString + "," + connectionGroupIdQueryString + ",'READ')";

            Queue<string> argNames = new Queue<string>();
            argNames.Enqueue("@groupname");

            Queue<string> args = new Queue<string>();
            args.Enqueue(groupName);

            //Insert the user into the entity table
            return InsertQuery(memberQueryString, argNames, args, ref excepts);
        }



        public bool InsertConnection(string groupName, string connectionName, string ip, string protocol, ref List<Exception> excepts)
        {
            const string connectionGroupIdQueryString =
                "(SELECT connection_group_id FROM guacamole_connection_group " +
                "WHERE connection_group_name = @groupname)";
                
            const string memberQueryString =
                "INSERT INTO guacamole_connection (connection_name, parent_id, protocol) " +
                "VALUES (@connectionname, " + connectionGroupIdQueryString + ", @protocol)";

            Queue<string> argNames = new Queue<string>();
            argNames.Enqueue("@groupname");
            argNames.Enqueue("@connectionname");
            argNames.Enqueue("@protocol");

            Queue<string> args = new Queue<string>();
            args.Enqueue(groupName);
            args.Enqueue(connectionName);
            args.Enqueue(protocol);

            //Insert the user into the entity table
            return InsertQuery(memberQueryString, argNames, args, ref excepts);
        }


        /// <summary>
        /// General format for running a insertion query on the guacamole database
        /// </summary>
        /// <returns><c>true</c>, if the values were inserted, <c>false</c> otherwise.</returns>
        /// <param name="queryString">Query string.</param>
        /// <param name="argNames">Argument names.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="excepts">Exceptions.</param>
        private bool InsertQuery(string queryString, Queue<string> argNames, Queue<string> args, ref List<Exception> excepts)
        {
            const string exceptMessage = "The database arguments and argument names are not the same size.";

            //Validate if the arguments and names are the correct amount
            if (args.Count != argNames.Count)
            {
                excepts.Add(new GuacamoleDatabaseException(exceptMessage));
                return false;
            }

            //Make a deep copy of the queues to ensure data consistancy
            Queue<String> copiedArgNames = new Queue<String>(argNames.ToArray());
            Queue<String> copiedArgs = new Queue<String>(args.ToArray());

            try
            {
                using (GuacamoleDatabaseConnector gdbc = new GuacamoleDatabaseConnector(ref excepts))
                {
                    using (MySqlCommand query = new MySqlCommand(queryString, gdbc.getConnection()))
                    {
                        query.Prepare();

                        //Add the agrument names and values NOTE: ORDER MATTERS
                        while (copiedArgs.Count > 0 && copiedArgNames.Count > 0)
                        {
                            Console.Error.Write("Name = " + copiedArgNames.Peek() + " Arg = " + copiedArgs.Peek() + ".\n\n");
                            query.Parameters.AddWithValue(copiedArgNames.Dequeue(), copiedArgs.Dequeue());
                        }
                        return (query.ExecuteNonQuery() > 0);
                    }
                }
            }
            catch (Exception e)
            {
                excepts.Add(e);
                return false;
            }
        }
    }
}
