using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Common.ModelXML.XML;


namespace Common.Managers
{

    [Serializable]
    public sealed class SimulationRunningConfig : ConfigBase
    {
        public string SIM_ID { get; set; }
        public string SIM_NAME { get; set; }
        public string SIM_DESCRIPTION { get; set; }
        public string SIM_PASSWORD { get; set; }

        public string APP_ROLE { get; set; } //Manager, Participant..
        public string APP_ID { get; set; } //The unique 10 char identifier for this application

        //DEM/CTM roles
        public static readonly string ROLE_MANAGER = "Manager";
        public static readonly string ROLE_PARTICIPANT = "Participant";

        public XMLSystemProfile HardwareProfile { get { return _systemProfile; }}
        public XMLAppSettings AppSettings { get; set; }


        //instantiate using hard coded static values
        public SimulationRunningConfig() 
            : base()
        {
            APP_ID = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper();
            APP_ROLE = ROLE_PARTICIPANT;

        }

        //instantitate by loading the application's settings from a database
        public SimulationRunningConfig(XMLAppSettings settings)
            : base()
        {
            APP_ID = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper();
            APP_ROLE = ROLE_PARTICIPANT;

            AppSettings = settings;

            //load network settings from AppSettings
            UDP_NETWORK_ADDRESS = settings.NetworkSettings.UDPNetworkAddress;
            UDP_NETWORK_BROADCAST_ADDRESS = settings.NetworkSettings.UDPNetworkBroadcastAddress;
            UDP_SIMULATION_APP_PORT = settings.NetworkSettings.UDPAppPort;
            TCP_SIMULATION_APP_PORT = settings.NetworkSettings.TCPAppPort;
        }










        public void setSimDescription(String description)
        {
            SIM_DESCRIPTION = description;
        }

    }
}
