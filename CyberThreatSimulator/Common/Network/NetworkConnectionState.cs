using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Common.Network
{
    /**
     * Maintains state data about a socket - holds the network stream and tcp client used to communicate
     * with the outside computer that is connected to this computers socket
     */

    
   

    public class NetworkConnectionState
    {
        public TcpClient TcpClient { get { return _client; } }
        public NetworkStream NetworkStream { get { return _stream; } }

        private TcpClient _client;
        private NetworkStream _stream;

        public NetworkConnectionState(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public NetworkConnectionState()
        {

        }

        public bool active()
        {

            if (_client == null)
                return false;
            else
                return _client.Connected;
        }
    }
}
