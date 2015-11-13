using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
using Common.ModelXML.XML;

using System.Data.SQLite;

using Common.Managers;
using Common.Network;

using DatabaseInterface;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;

namespace NetworkAttackSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /**
         * @author D'Mita Levy
         * @contact dlevy022@fiu.edu
         * @last 7/21/2015
         * 
         * WARNING: !! SQLite Interoperability DLL is unmanaged coded and must be present in the bin directory,
         *             in addition, the project must reference the managed dll System.Data.SQLite for SQLite to work,
         *             AND the project must be set to build for the x64 architecture
         * 
         **/


        /*
         * Specific classes for SA control and management
         */
        SimulationRunningConfig  _simConfig = new SimulationRunningConfig(); //this SA's config info
        SimulationManagementHelper _simManager; //manages simulation, threat handling etc.
        ConsoleUdpListener udpListener; //listens for udp packets on the multicast network
        DatabaseConnection databaseInterface; //connects to the database
        SimConsoleTcpListener _tcpServerListener;

        /*
         * MainWindow specific classes, modules, fields etc.
         */
        private ObservableConcurrentDictionary<string, XMLSimSetupNotification> _setupMessages; //simulation messages
        public ObservableConcurrentDictionary<string, XMLSimSetupNotification> SimSetupMessages { get { return _setupMessages; } set { } }


        private ObservableConcurrentDictionary<string, SimulationParticipant> _participantList; //simulation participants
        public ObservableConcurrentDictionary<string, SimulationParticipant> SimParticipants { get { return _participantList; } set { } }

        private bool simulationStarted = false; //send to true if the simulation has actually begun and is NOT waiting for participants to join
        private static int SIM_ID_MAX_LENGTH = 36;
        private static int SIM_NAME_MAX_LENGTH = 50;
        private static int SIM_ID_MIN_LENGTH = 5;
        private static long SIM_UPDATE_TIMEOUT = 20000; //time before a simulation is considered unreachable
        private const string DATA_SOURCE_STRING = "Data Source=..\\..\\Database\\Database.sqlite;";

     /*   private List<XMLSimSetupNotification> _simulations = new List<XMLSimSetupNotification>();
        public List<XMLSimSetupNotification> Simulations
        {
            get
            {
                return _simulations;
            }
            set
            {
                _simulations = value;
            }
        }*/

        public MainWindow()
        {
            InitializeComponent();

            //necessary for data binding
            this.DataContext = this;
            _setupMessages = new ObservableConcurrentDictionary<string, XMLSimSetupNotification>();
            _participantList = new ObservableConcurrentDictionary<string, SimulationParticipant>();

            LoadApplicationData(); // <---------start
        }

        public void LoadApplicationData()
        {
            //load Database Connection
            databaseInterface = new DatabaseConnection();

            //load Application Settings
            AppLoader appLoader = new AppLoader(databaseInterface, DATA_SOURCE_STRING);

            //apploader will create simulation config from database data
            _simConfig = new SimulationRunningConfig(appLoader.loadAppSettings());

           //_simConfig = new SimulationRunningConfig(); //load default hard coded config.

            //start listening for UDP Messages 
            Task.Factory.StartNew(() => startUdpListener());

        }

        private void JoinSimulation(object sender, RoutedEventArgs e)
        {
            Button simJoinBtn = (Button)sender;
            string simId = "" + simJoinBtn.Content;

            XMLSimSetupNotification simulation = new XMLSimSetupNotification();
            _setupMessages.TryGetValue(simId, out simulation);

            if(String.Equals(simulation.ManagerId, _simConfig.APP_ID))
            {
                MessageBox.Show("You are already a part of this simulation. Please click the simulation tab to for more info.");
                return;
            }
            if(simulation == null)
            {
                MessageBox.Show("An Error has occured...unable to join simulation - reference is null or empty.");
                return;
            }


            logToConsole("====Attempting to Join Sim=====");
            logToConsole("simulation: " + simulation.ManagerId);
            logToConsole("simulation: " + simulation.ManagerPort);
            logToConsole("simulation: " + simulation.ManagerIp);
            logToConsole("simulation: " + simulation.SimulationName);

            SimulationManagementHelper _simManager = new SimulationManagementHelper(_simConfig, DATA_SOURCE_STRING);
            _simManager.joinSimulation(simulation);
        }

       
        public void startUdpListener()
        {
            logToConsole(String.Format("Staring Udp Listener...Listening with address {0} to address {1} on port {2}", IPAddress.Any.ToString(), SimulationRunningConfig.UDP_NETWORK_BROADCAST_ADDRESS, SimulationRunningConfig.UDP_SIMULATION_APP_PORT));

            //start listening for UDP packets on the multicast network
            udpListener = new ConsoleUdpListener(IPAddress.Any.ToString(), SimulationRunningConfig.UDP_NETWORK_BROADCAST_ADDRESS, SimulationRunningConfig.UDP_SIMULATION_APP_PORT);
            udpListener.MessageReceived += new ConsoleUdpListener.MessageReceivedEvent(udpListener_MessageReceived);

            udpListener.Start();
        }

        public void udpListener_MessageReceived(string udpMessage)
        {
            //logToConsole(String.Format("Udp Listener Received message: {0}", udpMessage));

            if (String.IsNullOrEmpty(udpMessage))
            {
                //_logger.Warn("Warning UDP multicast messege from manager is NULL or empty!");
                logToConsole("Received empty message");
                return;
            }

            XmlSerializer deserializer = new XmlSerializer(typeof(MessageWrapper));
            MessageWrapper wrappedMessage;

            using (TextReader reader = new StringReader(udpMessage))
            {
                wrappedMessage = (MessageWrapper)deserializer.Deserialize(reader);
            }

            switch (wrappedMessage.MessageType)
            {
                case MessageBase.MESSAGE_SIM_SETUP:
                     XMLSimSetupNotification message = (XMLSimSetupNotification)wrappedMessage.Message;

                        if (_setupMessages.ContainsKey(message.SimulationId))
                        {
                            //SimSetupMessages.Remove(message.SimulationId);
                            //SimSetupMessages.Add(message.SimulationId, message);
                            //SimSetupMessages.
                        }
                        else
                        {
                         _setupMessages.Add(message.SimulationId, message);
                         }

                        //delete old simulations
                        DateTime now = System.DateTime.Now;
                        TimeSpan elapsed = wrappedMessage.Time.Subtract(now);

                            //iterate through dict...
                        if (elapsed.Seconds > SIM_UPDATE_TIMEOUT)
                         {
                            //remove old data...
                         }
                break;
            }    


        }

        //This SA will act as a Simulation Manager
        public void CreateSimulation(object sender, RoutedEventArgs e)
        {
            String simId = Sim_ID.Text;
            String simName = Sim_Name.Text;

            if(simId.Length > SIM_ID_MAX_LENGTH || simId.Length < SIM_ID_MIN_LENGTH){
                MessageBox.Show("Simulation ID requires at least 5-36 characters"); return;
            }

            if (simName.Length > SIM_NAME_MAX_LENGTH)
            {
                MessageBox.Show("Simulation Name must be less than 50 characters"); return;
            }
            else if (simName == "") Sim_Name.Text = simName = "Cyber Threat Simulation #" + simId;

            //start a sim manager
            _simConfig.APP_ROLE = SimulationRunningConfig.ROLE_MANAGER;
            _simConfig.SIM_ID = simId;
            _simConfig.SIM_NAME = simName;


            if(Sim_Description.Text == "")
                _simConfig.SIM_DESCRIPTION = "None";
            else
                _simConfig.SIM_DESCRIPTION = Sim_Description.Text;

            AppRoleTextBox.Text = "ROLE: MANAGER";

            //start a new simulation as the simulation manager
            _simManager = new SimulationManagementHelper(_simConfig, DATA_SOURCE_STRING);
            _simManager.startBroadcastingSimulation();

            //add manager to list

            //generate this managers sim application config and add to participant list
            XMLSimApplication saConfig = new XMLSimApplication();
            saConfig.ElementId = SimulationManagementHelper.generateElementId();
            saConfig.ParticipantId = _simConfig.APP_ID;
            saConfig.ParticipantIp = _simConfig.LOCAL_IP_ADDRESS;
            saConfig.ParticipantRole = _simConfig.APP_ROLE;
            saConfig.SystemProfile = _simConfig._systemProfile;
            saConfig.loadSimApplicationConfiguration(_simConfig);
            saConfig.Status = "Listening";

            //Note that the NetworkConnectionState is null because the manager will not be communicating with itself
            //In addition, SA should not communicate with each other unless sending through the UDP network, they only
            //communicate with the manager at the time of connection setup or rejoin if disconnected.
            SimulationParticipant manager = new SimulationParticipant(saConfig, new NetworkConnectionState());
            _participantList.Add(saConfig.ParticipantId, manager);

            startSimulationServer();

        }

        private void startSimulationServer()
        {
            logToConsole("Starting Simulation Server");
            _tcpServerListener = new SimConsoleTcpListener(IPAddress.Any, SimulationRunningConfig.TCP_SIMULATION_APP_PORT);
            _tcpServerListener.MessageReceived += new SimConsoleTcpListener.MessageReceivedEvent(tcpSimulationServer_MessageReceived); //does this block or thread?

            _tcpServerListener.Start();


        }

        //getsw all tcp messages
        public void tcpSimulationServer_MessageReceived(ConsoleTcpListenerArgs args)
        {
            
            if (String.IsNullOrEmpty(args.Message))
            {
                logToConsole("Console Tcp Listener received empty message");
                return;
            }

            XmlSerializer deserializer = new XmlSerializer(typeof(MessageWrapper));
            MessageWrapper wrappedMessage;

            //deserialize
            using (TextReader reader = new StringReader(args.Message))
            {
                wrappedMessage = (MessageWrapper)deserializer.Deserialize(reader);
            }

            if (wrappedMessage != null)
                handleSimServerRequests(wrappedMessage, args); //handle the message - pass to big switch statement
        }

        private void handleSimServerRequests(MessageWrapper wrappedMessage, ConsoleTcpListenerArgs args)
        {
            //retreive actual message payload and handle the different message types
            switch (wrappedMessage.MessageType)
            {
                case MessageBase.MESSAGE_JOIN_REQUEST:
                    XMLSimJoinRequest message = (XMLSimJoinRequest)wrappedMessage.Message;
                    if (message.valid())
                    {
                        try
                        {
                            //accept the connection from the SA
                            XMLSimJoinAcceptance joinAccept = new XMLSimJoinAcceptance();
                            joinAccept.ParticipantId = message.ParticipantId; //echo back
                            joinAccept.ParticipantIp = message.ParticipantIp;
                            joinAccept.ElementId = SimulationManagementHelper.generateElementId();
                            joinAccept.RequestConfig = true;

                            //wrap XML message with DEM header and send out
                            MessageWrapper wrap = new MessageWrapper();
                            wrap.ApplicationId = _simConfig.APP_ID;
                            wrap.ApplicationRole = _simConfig.APP_ROLE;
                            wrap.MessageType = MessageBase.MESSAGE_JOIN_ACCEPTANCE;
                            wrap.Message = joinAccept;

                            System.Console.WriteLine("Sending: \n" + wrap.toXmlString());
                            byte[] data = Encoding.UTF8.GetBytes(wrap.toXmlString());
                            NetworkStream netstream;
                            netstream = args.Client.GetStream();
                            netstream.Write(data, 0, data.Length);

                            String incoming = String.Empty;

                            bool done = false;
                            System.Console.WriteLine("Waiting for SA ");
                            //wait for the config from the SA
                            while (!done)
                            {
                                netstream = args.Client.GetStream();
                                if (netstream.DataAvailable && netstream != null)
                                {
                                    data = new Byte[args.Client.ReceiveBufferSize];
                                    netstream.Read(data, 0, (int)args.Client.ReceiveBufferSize);
                                    incoming = Encoding.UTF8.GetString(data);
                                    //System.Console.WriteLine("Received Data"+incoming);

                                    XmlSerializer deserializer = new XmlSerializer(typeof(MessageWrapper));
                                    MessageWrapper incomingWrappedMessage;

                                    //deserialize
                                    using (TextReader reader = new StringReader(incoming))
                                    {
                                        incomingWrappedMessage = (MessageWrapper)deserializer.Deserialize(reader);
                                    }

                                    //accepted to join the simulation, send config
                                    if (String.Equals(incomingWrappedMessage.MessageType, MessageBase.MESSAGE_SIM_APP_CONFIG))
                                    {
                                        System.Console.WriteLine("Received:" + incomingWrappedMessage.toXmlString());
                                        XMLSimApplication current = (XMLSimApplication)incomingWrappedMessage.Message;
                                        //add the SA to the list

                                        SimulationParticipant participant = new SimulationParticipant(current,new NetworkConnectionState(args.Client));

                                        _participantList.Add(current.ParticipantId, participant);
                                        done = true;

                                        if (_participantList.Values.Count > 1)
                                        {
                                            List<KeyValuePair<string,SimulationParticipant>> allParticipants = _participantList.ToList();
                                            List<XMLSimApplication> allApplications = new List<XMLSimApplication>();

                                            //build a list of applications to update the new app that joined
                                            foreach (var app in allParticipants)
                                            {
                                                allApplications.Add(app.Value.SimulationApplication);
                                            }

                                            XMLParticipantList updatedList = new XMLParticipantList();
                                            updatedList.Participants = allApplications;

                                            //wrap XML message with DEM header and send out
                                            wrap = new MessageWrapper();
                                            wrap.ApplicationId = _simConfig.APP_ID;
                                            wrap.ApplicationRole = _simConfig.APP_ROLE;
                                            wrap.MessageType = MessageBase.MESSAGE_UPDATE_PARTICPANT_LIST;
                                            wrap.Message = updatedList;

                                            //notifyParticipants(wrap);//tell everybody
                                        }
                                    }
                                }

                            }//end while

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        finally
                        {
                            args.Stream.Close();
                            args.Client.Close();
                        }
                    }
                    break;
            }

        }

        //used to notify all simulation participants - iterates through network streams and sends to each one
        private void notifyParticipants(MessageWrapper message)
        {
            //notify each participant - threaded since we dont want to block this thread if
            //connection is hung or erroneous
            List<KeyValuePair<string, SimulationParticipant>> allParticipants = _participantList.ToList();

            //build a list of applications to update the new app that joined
            foreach (var app in allParticipants)
            {
                SimulationParticipant current = app.Value;
                if (current.SimulationApplication.ParticipantId == _simConfig.SIM_ID) //skip yourself
                    continue;

                if (!current.StatefulSocket.active()) //inactive connection...
                {
                    _participantList.Remove(current.SimulationApplication.Status="Disconnected");
                    logToConsole("Error: Participant with App Id# "+current.SimulationApplication.ParticipantId+" is disconnected");
                }
             
                //send the message then
                Task.Factory.StartNew(() => send(current.StatefulSocket.TcpClient,message.toXmlString()));

            }

        }

        //send out tcp socket ONLY
        private void send(TcpClient client, String message)
        {

            System.Console.WriteLine("Sending..........");
            byte[] data = Encoding.UTF8.GetBytes(message);
            NetworkStream netstream = client.GetStream();
            netstream.Write(data, 0, data.Length);
            
        }

        //The user would like the Application to generate a unique Simulation ID
        private void GenerateID_Checked(object sender, RoutedEventArgs e)
        {
            //generate uuid to be used as simulation id
            String randomSimId = Guid.NewGuid().ToString().Replace("_","");
                randomSimId = randomSimId.Substring(0,SIM_ID_MIN_LENGTH).ToUpper();

            System.Console.WriteLine("The application has generated a random Simulation ID: "+randomSimId);
            Sim_ID.Text = randomSimId;
        }

        //Query the master table for the list of all tables and add them to the tree view
        private void ListDatabaseTree()
        {
            string table;

            databaseInterface.Connect(DATA_SOURCE_STRING);
            SQLiteDataReader query = databaseInterface.ExecuteQuery("SELECT * FROM sqlite_master WHERE type='table'");

            //read each table name and assign as tree view child
            while (query.Read())
            {
                table = (string)query["name"];

                TreeViewItem newChild = new TreeViewItem();
                newChild.Header = table;
                TedRoot.Items.Add(newChild);
            }
            databaseInterface.CloseConnection();
        }

        //show the table that was selected in the tree view
        private void ShowDatabase(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.TedTree.SelectedItem != null)
            {
                TreeViewItem selectedItem = (TreeViewItem)TedTree.SelectedItem;
                string table = selectedItem.Header.ToString();
                if (table.Equals("TED")) //ignore root item (db name)
                    return;

                SQLiteConnection connect = databaseInterface.Connect(DATA_SOURCE_STRING);
                SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + table + " WHERE 1", connect);
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                DataSet ds = new DataSet();
                da.SelectCommand = cmd;
                da.Fill(ds);

                tableDisplay.ItemsSource = ds.Tables[0].DefaultView;
                databaseInterface.CloseConnection();
            }

        }


        private void Query_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection connect = databaseInterface.Connect(DATA_SOURCE_STRING);
            SQLiteCommand cmd = new SQLiteCommand(SQLTextBox.Text, connect);
            SQLiteDataAdapter da = new SQLiteDataAdapter();
            DataSet ds = new DataSet();
            da.SelectCommand = cmd;
            da.Fill(ds);

            tableDisplay.ItemsSource = ds.Tables[0].DefaultView;
            databaseInterface.CloseConnection();
            SQLTextBox.Text = "";

        }

        private void Transact_Click(object sender, RoutedEventArgs e)
        {
            databaseInterface.Connect(DATA_SOURCE_STRING);
            databaseInterface.ExecuteUpdate(SQLTextBox.Text);
            databaseInterface.CloseConnection();
            SQLTextBox.Text = "";
        }

        private void Clear_SQL_TextBox(object sender, RoutedEventArgs e) { SQLTextBox.Text = ""; }
       
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e){ Close();}

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl item = sender as TabControl;
            if (item == null)
                return;
            TabItem select = item.SelectedItem as TabItem;
            if (select == null)
                return;

            if (select == tabConfig)
            {
                
            }
            else if (select == tabThreat)
            {
                
                ListDatabaseTree();
            }

            //switch (select.Header.ToString())
            //{
            //    case "Configuration":
            //        System.Console.WriteLine("Its me config here!");
            //        break;
            //    case "Threat Effects Database":
            //        System.Console.WriteLine("It's me TED here!");
            //        ListDatabaseTree();
            //        break;
            //}
        }

        private void logToConsole(string message)
        {
            System.Console.WriteLine("Navy TED: " + message);
        }

        private void stopSimulationManager()
        {
            if (_simManager == null)
            {
                MessageBox.Show("You must first create a simulation before using the stop button");
                return;
            }

            _simManager.stopBroadcastingSimulation();
            Sim_Name.Text = "";
            Sim_ID.Text = "";
        }

        //Stop simulation button click
        private void StopSimulation(object sender, RoutedEventArgs e)
        { 
            stopSimulationManager();

            _setupMessages.Remove(_simConfig.SIM_ID);//remove from list

            if (!simulationStarted)
            {
                //notify all the participants that the sim setup is done and there will be no simulation
            }
        }






    }
}
