using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Net.Sockets;
using System.Net;

using System.IO;
using System.Collections;

using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Reflection;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.ModelXML.XML;
using Common.Managers;
using Common.ModelXML;

namespace Common.Network
{
    public class NetworkConnection : IModel
    {
        Stream stream;
        TcpClient client;
        SimulationRunningConfig _appConfig;

        public NetworkConnection(SimulationRunningConfig config)
        {
            _appConfig = config;
        }

        public MessageWrapper generateMessageWrapper(string type)
        {
            MessageWrapper wrap = new MessageWrapper();
            wrap.ApplicationId = _appConfig.APP_ID;
            wrap.ApplicationRole = _appConfig.APP_ROLE;
            wrap.MessageType = type;
            return wrap;

        }

        public void sendTcpDataConnectionRequest(string msg, string address, int port)
        {
            client = new TcpClient(address,port);
            byte[] data = Encoding.UTF8.GetBytes(msg);
            stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            receive();
         
        }

        public void sendUdpBroadcast(string msg, string address, int port)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ipAddr = new IPEndPoint(IPAddress.Parse(address), port);
            //IPEndPoint ipAddr = new IPEndPoint(IPAddress.Broadcast, port);
            byte[] data = Encoding.Unicode.GetBytes(msg);
            client.Send(data, data.Length, ipAddr);
            client.Close();
        }

        public void send(string msg)
        {

            System.Console.WriteLine("Sending..........");
            byte[] data = Encoding.UTF8.GetBytes(msg);
            NetworkStream netstream = client.GetStream();
            netstream.Write(data, 0, data.Length);
        }

        public void receive()
        {
            String message = String.Empty;

            while (true)
            {
                NetworkStream netstream = client.GetStream();
                if (netstream.DataAvailable && netstream != null)
                {
                    byte[] data = new Byte[client.ReceiveBufferSize];
                    netstream.Read(data, 0, (int)client.ReceiveBufferSize);
                    message = Encoding.UTF8.GetString(data);

                    XmlSerializer deserializer = new XmlSerializer(typeof(MessageWrapper));
                    MessageWrapper wrappedMessage;

                    //deserialize
                    using (TextReader reader = new StringReader(message))
                    {
                        wrappedMessage = (MessageWrapper)deserializer.Deserialize(reader);
                    }

                    //accepted to join the simulation, send config
                    if (String.Equals(wrappedMessage.MessageType, MessageBase.MESSAGE_JOIN_ACCEPTANCE))
                    {
                        System.Console.WriteLine("Received: \n" + wrappedMessage.toXmlString());

                        XMLSimJoinAcceptance acceptance = (XMLSimJoinAcceptance) wrappedMessage.Message;

                        //generate and send this SA config
                        XMLSimApplication saConfig = new XMLSimApplication();
                        saConfig.ElementId = acceptance.ElementId;
                        saConfig.ParticipantId = _appConfig.APP_ID;
                        saConfig.ParticipantIp = _appConfig.LOCAL_IP_ADDRESS;
                        saConfig.ParticipantRole = _appConfig.APP_ROLE;
                        saConfig.SystemProfile = _appConfig._systemProfile;
                        saConfig.loadSimApplicationConfiguration(_appConfig);
                        saConfig.Status = "Waiting";

                        MessageWrapper wrapper = generateMessageWrapper(MessageBase.MESSAGE_SIM_APP_CONFIG);
                        wrapper.Message = saConfig;

                        //send sim manager this SA config
                        System.Console.WriteLine("Sending: \n"+ wrapper.toXmlString());
                        send(wrapper.toXmlString());
                        while (true)
                        {
                            //needs implementation -- waiting to start Simulation, set cyber terrain model, etc.
                        }
                    }

                    //received update participant list notification
                    else if (String.Equals(wrappedMessage.MessageType, MessageBase.MESSAGE_UPDATE_PARTICPANT_LIST))
                    {


                    }
                }

            }//end while
        }

        public void closeTcpConnect()
        {
            if (stream == null || client == null) 
                throw new NullReferenceException("NetworkConnection: cannot close Tcp connection - the connection is either empty or null");

            stream.Close();
            client.Close();
            
            
        }

    }

 
}
