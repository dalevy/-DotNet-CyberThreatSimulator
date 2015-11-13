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
    public class XMLSystemProfile
    {
        //Processor Information
        public string ProcessorManufacturer { get; set; }
        public string ProcessorName { get; set; }
        public UInt32 ProcessorMaxClockSpeed { get; set; }
        public string ProcessorID { get; set; }
        public UInt16 ProcessorRevision { get; set; }

        //Operating System Information
        public string OpSystemCaption { get; set; }
        public UInt16 OpSystemServicePackMajorVersion { get; set; }
        public UInt16 OpSystemServicePackMinorVersion { get; set; }
        public string OpSystemInstallDate { get; set; }
        public string OpSystemVersion { get; set; }
        public UInt64 OpSystemFreePhysicalMemory { get; set; }


        //Computer System Information
        public string SystemCaption { get; set; }
        public string SystemDescription { get; set; }
        public string SystemManufacturer { get; set; }
        public string SystemModel { get; set; }
        public UInt64 SystemTotalPhysicalMemory { get; set; }

        //CDROM Information
        [XmlElement("InstalledROMDrives")]
        public List<XMLHardwareCDROM> CDROMList { get; set; }

        public XMLSystemProfile()
        {

        }
    }
}
