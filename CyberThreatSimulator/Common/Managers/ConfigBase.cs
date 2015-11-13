using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Management;
using System.Text;
using System.Net;
using Common.ModelXML.XML;

namespace Common.Managers
{
    public abstract class ConfigBase
    {
        /**
         * Hardware Specific base class used to retrieve and initialize all system specific data
         * needed to create a Simulation Configuration.
         */

        //Network settings
        public static  string UDP_NETWORK_ADDRESS = "239.1.2.0";
        public static  string UDP_NETWORK_BROADCAST_ADDRESS = "239.1.2.255";
        public static  int UDP_SIMULATION_APP_PORT = 7414;
        public static  int TCP_SIMULATION_APP_PORT = 7424;

        public string LOCAL_IP_ADDRESS { get; set; }
        public IPAddress LOCAL_IP_ADDRESS_OBJECT { get; set; }

        public XMLSystemProfile _systemProfile;

        public ConfigBase()
        {
            LOCAL_IP_ADDRESS = getMyIpAddress();
            //buildXMLSystemProfile();
        }

        public string getMyIpAddress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    LOCAL_IP_ADDRESS_OBJECT = ip;
                    localIP = ip.ToString();
                }
            }

            return localIP;
        }


        public void updateSystemProfile()
        {

        }

        //build a list of all the hardware on this computer
        public void buildXMLSystemProfile()
        {
            _systemProfile = new XMLSystemProfile();

            //Get Processor info
            SelectQuery query = new SelectQuery(@"SELECT * FROM Win32_Processor");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject process in searcher.Get())
                {
                    _systemProfile.ProcessorManufacturer = (string)process["Manufacturer"];
                    _systemProfile.ProcessorName = (string)process["Name"];
                    _systemProfile.ProcessorMaxClockSpeed = (UInt32)process["MaxClockSpeed"];
                    _systemProfile.ProcessorID = (string)process["ProcessorID"];
                    _systemProfile.ProcessorRevision = (UInt16)process["Revision"];
                    System.Console.WriteLine("{0},{1},{2},{3},{4}", _systemProfile.ProcessorManufacturer, _systemProfile.ProcessorName, _systemProfile.ProcessorMaxClockSpeed, _systemProfile.ProcessorID, _systemProfile.ProcessorRevision);
                }
            }

            //Get CD Rom info
            query = new SelectQuery(@"SELECT * FROM Win32_CDROMDrive");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                _systemProfile.CDROMList = new List<XMLHardwareCDROM>();

                foreach (ManagementObject process in searcher.Get())
                {
                    //query each CDROM device and add to the list
                    XMLHardwareCDROM cd = new XMLHardwareCDROM();
                    cd.CDRomName = (string)process["Name"];
                    cd.CDRomCaption = (string)process["Caption"];
                    cd.CDRomDeviceID = (string)process["DeviceID"];
                    cd.CDRomDescription = (string)process["Description"];
                    _systemProfile.CDROMList.Add(cd);
                    System.Console.WriteLine("{0},{1},{2},{3}", cd.CDRomName, cd.CDRomCaption, cd.CDRomDeviceID, cd.CDRomDescription);
                }
            }

            
            //Get Operating System info
            query = new SelectQuery(@"SELECT * FROM Win32_OperatingSystem");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {

                foreach (ManagementObject process in searcher.Get())
                {
                    _systemProfile.OpSystemCaption = (string)process["Caption"];
                    _systemProfile.OpSystemServicePackMajorVersion = (UInt16)process["ServicePackMajorVersion"];
                    _systemProfile.OpSystemServicePackMinorVersion = (UInt16)process["ServicePackMinorVersion"];
                    _systemProfile.OpSystemInstallDate = (string)process["InstallDate"];
                    _systemProfile.OpSystemVersion = (string)process["Version"];
                    _systemProfile.OpSystemFreePhysicalMemory = (UInt64)process["FreePhysicalMemory"];
                    System.Console.WriteLine("{0},{1},{2},{3},{4},{5}", _systemProfile.OpSystemCaption, _systemProfile.OpSystemServicePackMajorVersion, _systemProfile.OpSystemServicePackMinorVersion, _systemProfile.OpSystemInstallDate, _systemProfile.OpSystemVersion, _systemProfile.OpSystemFreePhysicalMemory);

                }
            }

            query = new SelectQuery(@"SELECT * FROM Win32_ComputerSystem");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {

                foreach (ManagementObject process in searcher.Get())
                {
                    process.Get();
                    _systemProfile.SystemCaption = (string)process["Caption"];
                    _systemProfile.SystemDescription = (string)process["Description"];
                    _systemProfile.SystemManufacturer = (string)process["Manufacturer"];
                    _systemProfile.SystemModel = (string)process["Model"];
                    _systemProfile.SystemTotalPhysicalMemory = (UInt64)process["TotalPhysicalMemory"];
                    System.Console.WriteLine("{0},{1},{2},{3},{4}", _systemProfile.SystemCaption, _systemProfile.SystemDescription, _systemProfile.SystemManufacturer, _systemProfile.SystemModel, _systemProfile.SystemTotalPhysicalMemory);
                }
            }

        }//end buildhardwareprofile




    }

}
