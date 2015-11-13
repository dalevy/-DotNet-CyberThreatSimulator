using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Common.ModelXML.XML;

namespace Common.ModelXML.XML
{
    [Serializable]
    [XmlRoot("SimulatorModel")]
    public class MessageWrapper : MessageBase
    {
        [XmlElement(typeof(XMLParticipantList))]
        [XmlElement(typeof(XMLSimApplication))]
        [XmlElement(typeof(XMLSimJoinAcceptance))]
        [XmlElement(typeof(XMLSimJoinRequest))]
        [XmlElement(typeof(XMLSimSetupNotification))]
        [XmlElement(typeof(XMLHardwareCDROM))]
        [XmlElement(typeof(XMLSystemProfile))]
        public Object Message { get; set; } //wrapping (overloading) all message types so they include basic data (header) from MessageBase

        public MessageWrapper()
            :base()
        {

        }
    }
}
