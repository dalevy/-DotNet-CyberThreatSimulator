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
    public class XMLAppSettings
    {
        /**
         *  Reperesents the Application Settings (ip, port, network, user settings etc) for this SA
         */

          //user settings
          public string RetrieveHardwareProfile {get; set;}

          public XMLNetworkSettings NetworkSettings { get; set; }
          public XMLSystemProfile SystemProfile { get; set; }


        public XMLAppSettings()
        {

        }
    }
}
