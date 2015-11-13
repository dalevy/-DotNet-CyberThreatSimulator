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

using log4net;

namespace Common.Network
{
    public class ConsoleUdpListener
    {
        // Define a static logger variable
        //private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void MessageReceivedEvent(string msg);
        public event MessageReceivedEvent MessageReceived;

        private bool _isRunning = false;
        private UdpClient _udpClient = null;
        private IPEndPoint _endPoint = null;
        private string _multicastAddress;
        private string _localAddress;
        private int _port;

        public ConsoleUdpListener(string localAddress, string multicastAddress, int port)
        {
            _localAddress = localAddress;
            _multicastAddress = multicastAddress;
            this._port = port;
        }

        public bool Start()
        {
            try
            {
                //_logger.Debug(String.Format("Joining multicast group {0}:{1}", _multicastAddress, _port));

                _udpClient = new UdpClient();

                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.ExclusiveAddressUse = false;

                _endPoint = new IPEndPoint(IPAddress.Any, _port);
                _udpClient.Client.Bind(_endPoint);

                _udpClient.JoinMulticastGroup(IPAddress.Parse(_multicastAddress), IPAddress.Parse(_localAddress));

                //_logger.Debug(String.Format("\tJoined multicast group {0}:{1} successfully", _multicastAddress, _port));

                _isRunning = true;

                Receive();
            }
            catch (Exception ex)
            {
                //_logger.Error(String.Format("\tFailed to join multicast group {0}:{1}", _multicastAddress, _port), ex);
            }

            return _isRunning;
        }

        private void Receive()
        {
            if (_isRunning)
            {
                _udpClient.BeginReceive(new AsyncCallback(ReceiveDataCallback), null);
            }
        }

        private void ReceiveDataCallback(IAsyncResult ar)
        {
            Byte[] data = null;

            if (_isRunning)
            {
                if (_udpClient.Client == null)
                {
                    //_logger.Warn(String.Format("Udp Client socket is null.  Maybe OK when disposing object."));
                    return;
                }

                try
                {
                    data = _udpClient.EndReceive(ar, ref _endPoint);

                    if (data != null && data.Length != 0)
                    {
                        string msg = Encoding.Unicode.GetString(data);

                        //_logger.Debug(String.Format("Received message from multicast group {0}:{1}", _multicastAddress, _port));
                        //_logger.Debug(String.Format("\tUDP Message: {0}", msg));

                        SendMessageReceivedEvent(msg);
                    }
                }
                catch (Exception ex)
                {
                    //_logger.Error("Exception while receiving pdu message.", ex);
                }

                Receive();
            }
        }

        public void Stop()
        {
            _isRunning = false;

            if (_udpClient != null)
            {
                try
                {
                    _udpClient.DropMulticastGroup(IPAddress.Parse(_multicastAddress));
                    _udpClient.Client.Shutdown(SocketShutdown.Both);
                    _udpClient.Close();
                }
                catch (Exception ex)
                {
                    //_logger.Error("Exception while closing udp listener!", ex);
                }
            }

            //_logger.Debug(String.Format("Stopped UdpClient connection to join muticast group {0}:{1}", _multicastAddress, _port));
        }

        internal void SendMessageReceivedEvent(string msg)
        {
            MessageReceivedEvent mre = MessageReceived;
            if (mre != null)
            {
                //_logger.Debug("Sending message received event");
                mre(msg);
            }
        }
    }
}
