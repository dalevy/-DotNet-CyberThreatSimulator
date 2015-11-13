using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.ComponentModel;
using Common.Managers;

namespace Common.ModelXML.XML
{
    [Serializable]
    [XmlRoot("SimAppConfig")]
    public class XMLSimApplication
    {
        /*
         * The current state of this sim app
         */

        public string ParticipantId { get; set; }
        public string ParticipantIp { get; set; }
        public string ElementId { get; set; }
        public string ParticipantRole { get; set; }
        public string Status { get; set; }

        public XMLNetworkSettings NetworkSettings{get; set;}
        public XMLSystemProfile SystemProfile { get; set; }

        public XMLSimApplication()
        {

        }

        public void loadSimApplicationConfiguration(SimulationRunningConfig config)
        {
            if (config.AppSettings.NetworkSettings == null) throw new NullReferenceException("XMLSimApplication: Unable to load NetworkSettings - null");
            NetworkSettings = config.AppSettings.NetworkSettings;
            if (config.AppSettings.SystemProfile == null) throw new NullReferenceException("XMLSimApplication: Unable to load SystemProfile - null");
            SystemProfile = config.AppSettings.SystemProfile;
        }
    }
}
