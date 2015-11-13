using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.ModelXML.XML;
using Common.Network;

namespace Common.Managers
{
    public class SimulationParticipant
    {
        /**
         * The XMLSimApplication and NetworkConnectionState objects together
         * represent not only the XML profile of the internal system of the Simulation Participant
         * but also the return path for communicating with them  - the NetworkStream in the NetworkConnectionState object
         */

        public XMLSimApplication SimulationApplication { get { return _simApp; } }
        public NetworkConnectionState StatefulSocket { get { return _statefulSocket; } }

        private XMLSimApplication _simApp;
        private NetworkConnectionState _statefulSocket;

        public SimulationParticipant(XMLSimApplication app, NetworkConnectionState socketState)
        {
            _simApp = app;
            _statefulSocket = socketState;
        }

    }
}
