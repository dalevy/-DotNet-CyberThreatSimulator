using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using System.IO;


namespace DatabaseInterface
{
    public class DatabaseConnection
    {

        /**
         * @author D'Mita Levy
         * @contact dlevy022@fiu.edu
         * @last 7/20/2015
         * 
         * Interfaces with the Database. 
         * Note: Exception handling has NOT been implemente4d for this class
         *       The path location has been hardcoded, see datalocation();
         * 
         **/

        SQLiteConnection dbConnection;
        bool connOpen;

        public static void CreateDatabase(string name)
        {
            SQLiteConnection.CreateFile(name+".sqlite");
        }

        public SQLiteConnection Connect(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString)) throw new NullReferenceException("TEDConnection: connection string is null");

            dbConnection = new SQLiteConnection(connectionString);
            dbConnection.Open();
            connOpen = true;

            return dbConnection;
        }

        public void CloseConnection()
        {
            if (!IsOpenConnection())
                System.Console.WriteLine("TEDConnection: Cannot close Database Connection, the connection has not been created");
            else
            {
                dbConnection.Close();
                connOpen = false;
            }
        }

        public bool IsOpenConnection()
        {
            if (dbConnection == null || !connOpen)
                return false;
            else
                return true;

        }

        public SQLiteDataReader ExecuteQuery(String sql)
        {
            if (sql == null || sql == "") throw new NullReferenceException("TEDConnection: SQL string is empty or null");
            //else
                //if (!validated(sql)) throw new NullReferenceException("TEDConnection: SQL Syntax incorrect Please check console output for a list of errors");

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            return reader;
           
            /*
            while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);
            Console.ReadLine();*/
        }

        public void ExecuteUpdate(string sql)
        {
            if (sql == null && sql == "")throw new NullReferenceException("TEDConnection: SQL string is empty or null");
            //else
                //if (!validated(sql)) throw new NullReferenceException("TEDConnection: SQL Syntax incorrect Please check console output for a list of errors"); //will print its own errors.

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }

        public SQLiteConnection getConnection()
        {
            return dbConnection;
        }

        public bool validated(string sql)
        {
            bool valid = true;

            TSql100Parser parser = new TSql100Parser(false);
            IScriptFragment fragment;
            IList<ParseError> errors;
            fragment = parser.Parse(new StringReader(sql), out errors);
            if (errors != null && errors.Count > 0)
            {
                valid = false;
                //List<string> errorList = new List<string>();
                foreach (var error in errors)
                {
                    //errorList.Add(error.Message);
                    Console.WriteLine(error.Message);
                }
            }

            return valid;

        }
    }
}
