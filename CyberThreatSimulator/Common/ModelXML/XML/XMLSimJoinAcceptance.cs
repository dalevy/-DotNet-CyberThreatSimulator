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
    /*
     * The Simulation Manager sends this when the participant has joined the simulation successfully
     */

    [Serializable]
    [XmlRoot("SimJoinAcceptance")]
    public class XMLSimJoinAcceptance
    {
        public string ParticipantId { get; set; }
        public string ParticipantIp { get; set; }
        public string ElementId { get; set; }

        //get the computers config - usually only the first connection, reconnects due to disconnect would set this field to false and just allow join
        public bool RequestConfig { get; set; }

    }
}
