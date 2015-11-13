using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

using DatabaseInterface;
using Common.Network;
using Common.ModelXML.XML;
using System.Net;
using Common.ModelXML;

namespace Common.Managers
{
    public class SimulationManagementHelper : IModel
    {
        /**
         * @author D'Mita Levy
         * @contact dlevy022@fiu.edu
         * @last 7/21/2015
         * 
         * Starts the Simulation Helper class that handles all simulation events,
         * including activating the appropriate simulation threat modules,
         * communicating with the database and start/stop of simulations
         * 
         **/

        public DatabaseConnection Interface;
        public SimulationRunningConfig _appConfig;
        Timer updateTimer;
        
        string tedConnectString;
        private int HEARTBEAT_INTERVAL = 1000;
        
        
        public SimulationManagementHelper(SimulationRunningConfig config, string tedDatasource)
        {
            setSimConfig(config);
            tedConnectString = tedDatasource;
            Interface = new DatabaseConnection();
            
        }


        //helper method to be wrapped for threading
        private void broadCastSimulation()
        {
            //log this simulation to the database
            Log_Simulation(tedConnectString);

            //network update (heartbeat) timer
            updateTimer = new System.Timers.Timer();
            updateTimer.Elapsed += new ElapsedEventHandler(broadCastNewSimulation);
            updateTimer.Interval = HEARTBEAT_INTERVAL;
            updateTimer.Enabled = true;
        }

        //assuming this SA is a manager it will broadcast the simulation data needed to connect to a simulation
        public void broadCastNewSimulation(object source, ElapsedEventArgs e)
        {
            //build XML sim setup message
            XMLSimSetupNotification setup = new XMLSimSetupNotification();
            setup.SimulationId = _appConfig.SIM_ID;
            setup.SimulationName = _appConfig.SIM_NAME;
            setup.ManagerIp = _appConfig.LOCAL_IP_ADDRESS;
            setup.ManagerPort = SimulationRunningConfig.TCP_SIMULATION_APP_PORT;
            setup.ManagerId = _appConfig.APP_ID;
            setup.Secure = false;
            //setup.Password = "Password"
            setup.SimulationDescription = _appConfig.SIM_DESCRIPTION;

            //wrap XML message with DEM header and send out
            MessageWrapper wrap = new MessageWrapper();
            wrap.ApplicationId = _appConfig.APP_ID;
            wrap.ApplicationRole = _appConfig.APP_ROLE;
            wrap.MessageType = MessageBase.MESSAGE_SIM_SETUP;
            wrap.Message = setup;

            //setup.printToConsole();

            NetworkConnection netPipe = new NetworkConnection(_appConfig);
            netPipe.sendUdpBroadcast(wrap.toXmlString(), SimulationRunningConfig.UDP_NETWORK_BROADCAST_ADDRESS, SimulationRunningConfig.UDP_SIMULATION_APP_PORT);
        }

        //broadcasts the current status of this SA
        public void broadCastStatus(object source, ElapsedEventArgs e)
        {
          
        }

        public MessageWrapper generateMessageWrapper(string type)
        {
            MessageWrapper wrap = new MessageWrapper();
            wrap.ApplicationId = _appConfig.APP_ID;
            wrap.ApplicationRole = _appConfig.APP_ROLE;
            wrap.MessageType = type;
            return wrap;

        }

        private void requestJoinSimulation(XMLSimSetupNotification simulation)
        {

            XMLSimJoinRequest joinRequest = new XMLSimJoinRequest();
            joinRequest.ParticipantId = _appConfig.APP_ID;
            joinRequest.ParticipantIp = _appConfig.LOCAL_IP_ADDRESS;

            //wrap XML message with Communications header and send out
            MessageWrapper wrap = new MessageWrapper();
            wrap.ApplicationId = _appConfig.APP_ID;
            wrap.ApplicationRole = _appConfig.APP_ROLE;
            wrap.MessageType = MessageBase.MESSAGE_JOIN_REQUEST;
            wrap.Message = joinRequest;

            System.Console.WriteLine("Sending: " + wrap.toXmlString());

            NetworkConnection netPipe = new NetworkConnection(_appConfig);
            netPipe.sendTcpDataConnectionRequest(wrap.toXmlString(), simulation.ManagerIp, simulation.ManagerPort);

        }


        public void startBroadcastingSimulation()
        {
            //start sending out simulation broadcasts on udp
            Task.Factory.StartNew(() => broadCastSimulation());

        }

        public void joinSimulation(XMLSimSetupNotification simulation)
        {
             //start sending out simulation broadcasts on udp
            Task.Factory.StartNew(() => requestJoinSimulation(simulation));
        }

        public void stopBroadcastingSimulation()
        {
            updateTimer.Enabled = false;
            updateTimer.Close();
        }

        //adds the current simulation to the database simconfigs table
        public void Log_Simulation(string datasource)
        {
            string date = DateTime.Now.ToString(); //datetime is saved as string for simplicity in the database
            string UPDATE_CONFIG = "INSERT INTO SimConfigs (date,simid,name,description) VALUES ('"+date+"','"+_appConfig.SIM_ID+"','"+_appConfig.SIM_NAME+"','"+_appConfig.SIM_DESCRIPTION+"')";

            Interface.Connect(datasource);
            Interface.ExecuteUpdate(UPDATE_CONFIG);
            Interface.CloseConnection();
            
        }

        //returns a random element Id
        public static String generateElementId()
        {
           return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper();
        }

        public void setSimConfig(SimulationRunningConfig config)
        {
            if (config == null) throw new NullReferenceException("Simulation Manager: Cannot set Simulation Configuration, the config is null");
            _appConfig = config;
        }

        public SimulationRunningConfig getSimConfig()
        {
            return _appConfig;
        }


        private class SimulationServer
        {


        }
    }
}
