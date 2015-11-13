using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using TEDInterface;
using System.Data.SQLite;

using TEDSQLite;

namespace TEDPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string TED_DATA_SOURCE_STRING = "Data Source=TED.sqlite;";

        TEDInterface.TEDConnection dbConnect = new TEDInterface.TEDConnection();
        
        public MainWindow()
        {
            InitializeComponent();
            //TEDInterface.TEDConnection.CreateDatabase("meowmeow");
            ListDatabaseTree();
           
        }

        //Query the master table for the list of all tables and add them to the tree view
        private void ListDatabaseTree()
        {
            string table;

            dbConnect.Connect(TED_DATA_SOURCE_STRING);
            SQLiteDataReader query = dbConnect.ExecuteQuery("SELECT * FROM sqlite_master WHERE type='table'");

            //read each table name and assign as tree view child
            while (query.Read())
            {
                table = (string)query["name"];

                TreeViewItem newChild = new TreeViewItem();
                    newChild.Header = table;
                    dbRoot.Items.Add(newChild);
            }
            dbConnect.CloseConnection();
        }

        //show the table that was selected in the tree view
        private void ShowDatabase(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = (TreeViewItem)dbTree.SelectedItem;
            string table = selectedItem.Header.ToString();
            if (table.Equals("TED")) //ignore root item (db name)
                return;

            SQLiteConnection connect = dbConnect.Connect(TED_DATA_SOURCE_STRING);
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + table + " WHERE 1", connect);
            SQLiteDataAdapter da = new SQLiteDataAdapter();
            DataSet ds = new DataSet();
            da.SelectCommand = cmd;
            da.Fill(ds);

            tableDisplay.ItemsSource = ds.Tables[0].DefaultView;
            dbConnect.CloseConnection();

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection connect = dbConnect.Connect(TED_DATA_SOURCE_STRING);
            SQLiteCommand cmd = new SQLiteCommand(queryText.Text, connect);
            SQLiteDataAdapter da = new SQLiteDataAdapter();
            DataSet ds = new DataSet();
            da.SelectCommand = cmd;
            da.Fill(ds);

            tableDisplay.ItemsSource = ds.Tables[0].DefaultView;
            dbConnect.CloseConnection();
            queryText.Text = "";

        }

        private void Transact_Click(object sender, RoutedEventArgs e)
        {
            dbConnect.Connect(TED_DATA_SOURCE_STRING);
            dbConnect.ExecuteUpdate(transactText.Text);
            dbConnect.CloseConnection();
            transactText.Text = "";
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
