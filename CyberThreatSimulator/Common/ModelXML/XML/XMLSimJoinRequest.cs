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
    [XmlRoot("SimJoinRequest")]
    public class XMLSimJoinRequest
    {
         /**
        * Requests to join a simulation
        */
        public string ParticipantId { get; set; }
        public string ParticipantIp { get; set; }
        public string Password { get; set; }

        //valid if all valu
        public bool valid()
        {
            if (String.IsNullOrEmpty(ParticipantIp) || String.IsNullOrEmpty(ParticipantId))
                return false;
            else
                return true;
        }

        public XMLSimJoinRequest(){} //default constructor
    }
}
