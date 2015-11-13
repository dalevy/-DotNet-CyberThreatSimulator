using System;
using System.Collections.Generic;
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

using log4net;

namespace Common.Network
{
    public class SimConsoleTcpListener
    {
        // Define a static logger variable
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void MessageReceivedEvent(ConsoleTcpListenerArgs args);
        public event MessageReceivedEvent MessageReceived;

        private bool isRunning = true;
        private Thread listenerThread;
        private TcpListener listener = null;
        private IPAddress _address;
        private int _port;

        private ManualResetEvent _threadStarted = new ManualResetEvent(false);

        public SimConsoleTcpListener(IPAddress address, int port)
        {
            _address = address;
            _port = port;
        }

        public bool Start()
        {
            if (listenerThread == null || !listenerThread.IsAlive)
            {
                isRunning = true;
                listenerThread = new Thread(new ThreadStart(tcpListenerThread));
                listenerThread.IsBackground = true;
            }

            if (!listenerThread.IsAlive)
            {
                listenerThread.Start();
            }

            if (_threadStarted.WaitOne(2000))
            {
                _logger.Debug("Started tcp listener thread");
                System.Console.WriteLine("Started tcp listener thread");
            }
            else
            {
                _logger.Debug("Falied to start tcp listener thread!");
                System.Console.WriteLine("Falied to start tcp listener thread!");
            }

            return listenerThread.IsAlive;
        }

        public void Stop()
        {
            isRunning = false;

            if (listener != null)
            {
                listener.Stop();
            }

            _logger.Debug("Stopped tcp listener thread");
        }

        private void tcpListenerThread()
        {
            try
            {
                //Create the Tcp listener (Server)
                listener = new TcpListener(_address, _port);
                listener.Start();

                _threadStarted.Set();

                //listen for requests until told to stop
                while (isRunning)
                {
                    if (!listener.Pending())
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        _logger.Debug("Accepting tcp connection");
                        System.Console.WriteLine("Accepting tcp connection");

                        TcpClient tcpClient = listener.AcceptTcpClient();

                        Thread clientThread = new Thread(new ParameterizedThreadStart(tcpClietThread));
                        clientThread.IsBackground = true;
                        clientThread.Start(tcpClient);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Tcp listener thread exception.", ex);
                System.Console.WriteLine("Tcp listener thread exception." + ex);
            }
            finally
            {
                //cleanup
                if (listener != null)
                {
                    try
                    {
                        listener.Stop();
                        listener = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception when stopping Http listener object.", ex);
                    }
                }
            }
        }

        private void tcpClietThread(object client)
        {
            TcpClient tcpClient = (TcpClient)client;

            if (tcpClient != null)
            {
                NetworkStream ns = tcpClient.GetStream();

                if (ns.CanRead)
                {
                    _logger.Debug("Reading tcp message");
                    System.Console.WriteLine("Reading tcp message");

                    byte[] bytes = new byte[tcpClient.ReceiveBufferSize];

                    string message = "";

                    try
                    {
                        ns.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
                        message = Encoding.UTF8.GetString(bytes).Trim(new char[] { '\0', '\n', '\t', '\r' });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception whiile reading tcp network stream", ex);
                        System.Console.WriteLine("Exception while reading tcp network steream " + ex);
                    }

                    _logger.Debug("tcp message: " + message);
                    System.Console.WriteLine("tcp message: " + message);

                    // Fire message received event...
                    MessageReceivedEvent mre = MessageReceived;
                    if (mre != null)
                    {
                        mre(new ConsoleTcpListenerArgs(message,ns,tcpClient));
                    }
                }

                //ns.Close();

                //tcpClient.Close();
            }
        }
    }

    //modified 8/4/2015 to also provide the stream and client so the server can send back to the client
    public class ConsoleTcpListenerArgs : EventArgs
    {
        public string Message { get; internal set; }
        public NetworkStream Stream { get; internal set; }
        public TcpClient Client {get; internal set; }

        internal ConsoleTcpListenerArgs(string message, NetworkStream sock, TcpClient tcpclient)
        {
            Message = message;
            Stream = sock;
            Client = tcpclient;
        }
    }
}
