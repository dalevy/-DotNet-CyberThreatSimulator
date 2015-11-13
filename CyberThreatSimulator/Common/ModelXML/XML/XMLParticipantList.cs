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
    [XmlRoot("ParticipantList")]
    public class XMLParticipantList
    {
        public List<XMLSimApplication> Participants { get; set; }

        public XMLParticipantList()
        {

        }
    }
}
