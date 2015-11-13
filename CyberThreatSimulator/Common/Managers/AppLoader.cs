using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseInterface;
using System.Data.SQLite;
using System.Data;
using System.Management.Instrumentation;
using System.Management;
using Common.ModelXML.XML;

namespace Common.Managers
{
    public class AppLoader
    {
        DatabaseConnection dbConnect;
        public XMLAppSettings _appSettings;
        string dbConnectString;

        public AppLoader(DatabaseConnection connect, string connectionString)
        {
            dbConnect = connect;
            dbConnectString = connectionString;
        }

        public XMLAppSettings loadAppSettings()
        {
            if (dbConnect == null) throw new NullReferenceException("AppLoader: Unable to load application settings - database connection is null or empty");
            System.Console.WriteLine("AppLoader: Building system profile");

            //1. Get application settings
            string appSettingsQuery = "SELECT * FROM AppSettings WHERE 1";

            SQLiteConnection connect = dbConnect.Connect(dbConnectString);
            SQLiteDataReader query = dbConnect.ExecuteQuery(appSettingsQuery);

            _appSettings = new XMLAppSettings();

            while (query.Read())
            {
                _appSettings.RetrieveHardwareProfile = (string)query["Retrieve_Hardware_Profile"];

            }
            
            //2. Retrieve network settings
            string networkSettingsQuery = "SELECT * FROM NetworkSettings WHERE 1";
            query = dbConnect.ExecuteQuery(networkSettingsQuery);
            _appSettings.NetworkSettings = new XMLNetworkSettings();

            while (query.Read())
            {
                _appSettings.NetworkSettings.TCPAppPort = (int)query["TCP_App_Port"];
                _appSettings.NetworkSettings.UDPAppPort = (int)query["UDP_App_Port"];
                _appSettings.NetworkSettings.UDPNetworkAddress = (string)query["UDP_Network_Address"];
                _appSettings.NetworkSettings.UDPNetworkBroadcastAddress = (string)query["UDP_Network_Broadcast_Address"];
                _appSettings.NetworkSettings.SIMMessageHeartbeatInterval = (int)query["SIM_Message_Heartbeat_Interval"];
                _appSettings.NetworkSettings.SIMSetupHeartbeatInterval = (int)query["SIM_Setup_Heartbeat_Interval"];
            }


            //3a. System profile needs to be retrieved (due to either first run, or something changed)
            if (String.Equals(_appSettings.RetrieveHardwareProfile, "YES"))
            {
                //build system profile from Windows Management Instrumentation
                _appSettings.SystemProfile = buildXMLSystemProfile();
                updateDatabaseSystemProfile(dbConnect, _appSettings.SystemProfile);

                //set retrieve hardware profile to 'NO' since it is now up to date
                //this way it will not run multitple times since buildXMLSystemProfile() is expensive
                string noRetrieveUpdate = "UPDATE AppSettings SET Retrieve_Hardware_Profile ='NO' WHERE 1";
                dbConnect.ExecuteUpdate(noRetrieveUpdate);
            }
            
            //3b. System profile is already in the database, so just load from db
            else
            {
                _appSettings.SystemProfile = loadSystemProfileFromDatabase(dbConnect);
            }

            dbConnect.CloseConnection();

            return _appSettings;

        }

        public XMLSystemProfile loadSystemProfileFromDatabase(DatabaseConnection database)
        {

            if (database == null) throw new NullReferenceException("AppLoader: Database connection is null");
            //connection not open so open it
            if (database.IsOpenConnection() == false)
                dbConnect.Connect(dbConnectString);

            XMLSystemProfile profile = new XMLSystemProfile();

            string query = "SELECT * FROM SystemProfile WHERE 1";

           SQLiteDataReader reader = dbConnect.ExecuteQuery(query);


            while (reader.Read())
            {
                profile.ProcessorManufacturer = (string)reader["Processor_Manufacturer"];
                profile.ProcessorName = (string)reader["Processor_Name"];
                //profile.ProcessorMaxClockSpeed = (UInt32)reader["Processor_Max_ClockSpeed"];
                profile.ProcessorID = (string)reader["Processor_ID"];
                //profile.ProcessorRevision = (UInt16)reader["Processor_Revision"];
                profile.OpSystemCaption = (string)reader["OpSystem_Caption"];
                //profile.OpSystemServicePackMajorVersion = (UInt16)reader["OpSystem_ServicePack_MajorVersion"];
                //profile.OpSystemServicePackMinorVersion = (UInt16)reader["OpSystem_ServicePack_MinorVersion"];
                profile.OpSystemInstallDate = (string)reader["OpSystem_Install_Date"];
                profile.OpSystemVersion = (string)reader["OpSystem_Version"];
                //profile.OpSystemFreePhysicalMemory = (UInt64)reader["OpSystem_Free_Physical_Memory"];
                profile.SystemCaption = (string)reader["System_Caption"];
                profile.SystemDescription = (string)reader["System_Description"];
                profile.SystemManufacturer = (string)reader["System_Manufacturer"];
                profile.SystemModel = (string)reader["System_Model"];
                //profile.SystemTotalPhysicalMemory = (UInt64)reader["System_Total_Physical_Memory"];

            }

            return profile;

        }

        public void updateDatabaseSystemProfile(DatabaseConnection database, XMLSystemProfile profile)
        {
            if(database == null) throw new NullReferenceException("AppLoader: Database connection is null");
            //connection not open so open it
            if(database.IsOpenConnection() == false)
                 dbConnect.Connect(dbConnectString);

            
            StringBuilder update = new StringBuilder();
            update.Append("INSERT INTO SystemProfile ");
            update.Append("(Processor_Manufacturer,Processor_Name ,Processor_Max_ClockSpeed, Processor_ID ,Processor_Revision ,OpSystem_Caption ,"+
                            "OpSystem_ServicePack_MajorVersion ,OpSystem_ServicePack_MinorVersion ,OpSystem_Install_Date ,OpSystem_Version ,"+
                            "OpSystem_Free_Physical_Memory , System_Caption ,System_Description ,System_Manufacturer ,System_Model ,System_Total_Physical_Memory)");
            update.Append("VALUES ");
            update.Append("(");
            update.Append("'"+profile.ProcessorManufacturer+"',");
            update.Append("'" + profile.ProcessorName + "',");
            update.Append("'" + profile.ProcessorMaxClockSpeed + "',");
            update.Append("'" + profile.ProcessorID + "',");
            update.Append("'" + profile.ProcessorRevision + "',");
            update.Append("'" + profile.OpSystemCaption + "',");
            update.Append("'" + profile.OpSystemServicePackMajorVersion + "',");
            update.Append("'" + profile.OpSystemServicePackMinorVersion + "',");
            update.Append("'" + profile.OpSystemInstallDate + "',");
            update.Append("'" + profile.OpSystemVersion + "',");
            update.Append("'" + profile.OpSystemFreePhysicalMemory+ "',");
            update.Append("'" + profile.SystemCaption+ "',");
            update.Append("'" + profile.SystemDescription + "',");
            update.Append("'" + profile.SystemManufacturer + "',");
            update.Append("'" + profile.SystemModel + "',");
            update.Append("'" + profile.SystemTotalPhysicalMemory + "'");
            update.Append(");");

            dbConnect.ExecuteUpdate(update.ToString());


        }


        //build a list of all the hardware on this computer
        public XMLSystemProfile buildXMLSystemProfile()
        {
            XMLSystemProfile systemProfile = new XMLSystemProfile();

            //Get Processor info
            SelectQuery query = new SelectQuery(@"SELECT * FROM Win32_Processor");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject process in searcher.Get())
                {
                    systemProfile.ProcessorManufacturer = (string)process["Manufacturer"];
                    systemProfile.ProcessorName = (string)process["Name"];
                    systemProfile.ProcessorMaxClockSpeed = (UInt32)process["MaxClockSpeed"];
                    systemProfile.ProcessorID = (string)process["ProcessorID"];
                    systemProfile.ProcessorRevision = (UInt16)process["Revision"];
                    System.Console.WriteLine("{0},{1},{2},{3},{4}", systemProfile.ProcessorManufacturer, systemProfile.ProcessorName, systemProfile.ProcessorMaxClockSpeed, systemProfile.ProcessorID, systemProfile.ProcessorRevision);
                }
            }

            //Get CD Rom info
            query = new SelectQuery(@"SELECT * FROM Win32_CDROMDrive");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                systemProfile.CDROMList = new List<XMLHardwareCDROM>();

                foreach (ManagementObject process in searcher.Get())
                {
                    //query each CDROM device and add to the list
                    XMLHardwareCDROM cd = new XMLHardwareCDROM();
                    cd.CDRomName = (string)process["Name"];
                    cd.CDRomCaption = (string)process["Caption"];
                    cd.CDRomDeviceID = (string)process["DeviceID"];
                    cd.CDRomDescription = (string)process["Description"];
                    systemProfile.CDROMList.Add(cd);
                    System.Console.WriteLine("{0},{1},{2},{3}", cd.CDRomName, cd.CDRomCaption, cd.CDRomDeviceID, cd.CDRomDescription);
                }
            }


            //Get Operating System info
            query = new SelectQuery(@"SELECT * FROM Win32_OperatingSystem");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {

                foreach (ManagementObject process in searcher.Get())
                {
                    systemProfile.OpSystemCaption = (string)process["Caption"];
                    systemProfile.OpSystemServicePackMajorVersion = (UInt16)process["ServicePackMajorVersion"];
                    systemProfile.OpSystemServicePackMinorVersion = (UInt16)process["ServicePackMinorVersion"];
                    systemProfile.OpSystemInstallDate = (string)process["InstallDate"];
                    systemProfile.OpSystemVersion = (string)process["Version"];
                    systemProfile.OpSystemFreePhysicalMemory = (UInt64)process["FreePhysicalMemory"];
                    System.Console.WriteLine("{0},{1},{2},{3},{4},{5}", systemProfile.OpSystemCaption, systemProfile.OpSystemServicePackMajorVersion, systemProfile.OpSystemServicePackMinorVersion, systemProfile.OpSystemInstallDate, systemProfile.OpSystemVersion, systemProfile.OpSystemFreePhysicalMemory);

                }
            }

            query = new SelectQuery(@"SELECT * FROM Win32_ComputerSystem");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {

                foreach (ManagementObject process in searcher.Get())
                {
                    process.Get();
                    systemProfile.SystemCaption = (string)process["Caption"];
                    systemProfile.SystemDescription = (string)process["Description"];
                    systemProfile.SystemManufacturer = (string)process["Manufacturer"];
                    systemProfile.SystemModel = (string)process["Model"];
                    systemProfile.SystemTotalPhysicalMemory = (UInt64)process["TotalPhysicalMemory"];
                    System.Console.WriteLine("{0},{1},{2},{3},{4}", systemProfile.SystemCaption, systemProfile.SystemDescription, systemProfile.SystemManufacturer, systemProfile.SystemModel, systemProfile.SystemTotalPhysicalMemory);
                }
            }

            return systemProfile;

        }//end buildhardwareprofile


    }
}
