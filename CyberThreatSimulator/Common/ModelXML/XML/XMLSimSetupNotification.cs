using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.ComponentModel;


namespace Common.ModelXML.XML
{
    [Serializable]
    [XmlRoot("SimSetupNotification")]
    public class XMLSimSetupNotification
    {

        /**
        * The purpose of these settings is so that all simulation applications
        * that are listening on on the default multicast network for the Navy TED
        * can then initiate a stateful connection to the Simulation Manager of their 
        * choice using TCP, negotiate Simulation setup information, and then return to listening
        * for UDP updates on the default multicast network
        */
        public string SimulationId { get; set; }
        public string SimulationName { get; set; }
        public string SimulationDescription { get; set; }

        public string ManagerIp { get; set; }
        public int ManagerPort { get; set; }
        public string ManagerId { get; set; }

        public bool Secure { get; set; }
        public string Password { get; set; }

        public XMLSimSetupNotification(){} //default constructor
    }
}
