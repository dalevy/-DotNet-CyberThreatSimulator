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
    [XmlRoot("NetworkSettings")]
    public class XMLNetworkSettings
    {
        //network settings
        [XmlElement("MulticastNetworkAddress")]
        public string UDPNetworkAddress { get; set; }
        [XmlElement("MulticastBroadcastAddress")]
        public string UDPNetworkBroadcastAddress { get; set; }
        [XmlElement("MulticastPortNumber")]
        public int UDPAppPort { get; set; }
        [XmlElement("TCPPortNumber")]
        public int TCPAppPort { get; set; }
        [XmlElement("SetupInterval")]
        public int SIMSetupHeartbeatInterval { get; set; } //how often sim setup notifications are transmitted
        [XmlElement("UpdateInterval")]
        public int SIMMessageHeartbeatInterval { get; set; } //how often sim messages in general are transmitted

        public XMLNetworkSettings()
        {
        }
    }
}
