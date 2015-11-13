using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;


namespace TEDSQLite
{
    public class TEDConnection
    {
        SQLiteConnection dbConnection;
        bool connOpen;

        public static void CreateDatabase(string name)
        {
            SQLiteConnection.CreateFile(name+".sqlite");
        }

        public string datalocation()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return "Data Source=TED.sqlite;";
        }

        public SQLiteConnection Connect(string connectionString)
        {
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

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            return reader;
           
            /*
            while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);
            Console.ReadLine();*/
        }

        public void ExecuteUpdate(String sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }

        public SQLiteConnection getConnection()
        {
            return dbConnection;
        }
    }
}
