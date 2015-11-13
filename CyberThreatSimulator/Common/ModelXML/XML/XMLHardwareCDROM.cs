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
    [XmlRoot("HardwareProfile")]
    public class XMLHardwareCDROM
    {

        public string CDRomName { get; set; }
        public string CDRomCaption { get; set; }
        public string CDRomDeviceID { get; set; }
        public string CDRomDescription { get; set; }

        public XMLHardwareCDROM()
        {

        }
    }
}
