using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using EngineName.Components;
using EngineName.Core;
using EngineName.Utils;
using Lidgren.Network;
using Microsoft.Xna.Framework;
namespace EngineName.Systems
{
    public class NetworkSystem : EcsSystem
    {
        private NetPeer _peer;
        private NetPeerConfiguration _config;
        private NetClient _client;
        private int _localport = 50001;
        private int _searchport = 50001;
        private NetIncomingMessage _msg;
        private bool _bot = false; 

        /// <summary>Inits networkssystems configures settings for lidgrens networks framework.</summary>
        public override void Init()
        {
            _config = new NetPeerConfiguration("Sap6_Networking")
            {
                Port = _localport,
                AcceptIncomingConnections = true
                
                
            };
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            _config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            _peer = new NetPeer(_config);
            _peer.Start();
            _peer.DiscoverLocalPeers(_searchport);

            if (!_bot) { 
                Game1.Inst.Scene.OnEvent("send_to_peer", data => this.SendMessage((string)data));
            }

            Debug.WriteLine("Listening on " + _config.Port.ToString());
            base.Init();
        }


        ///For testing only
        public void Bot()
        {
            _bot = true;
            int counter = 0;
            _localport = 50002;
            Init();
            while (true)
            {
                counter++;
                Thread.Sleep(1000);
                MessageLoop();
                if (counter % 5 == 0 && counter != 0)
                {
                    counter++;
                    SendObject(new CTransform() {Position = new Vector3(2,4,5)});
                    SendMessage("hej from bot " + counter);
                }

            }
        }
        /// <summary>Checks if have peers, so its possible to send stuff</summary>
        private bool havePeers()
        {
            return _peer.Connections != null && _peer.Connections.Count > 0;
        }
        /// <summary>Send information about newly connected peer to all other peers </summary>
        public void SendPeerInfo(IPAddress ip, int port)
        {
            if (!havePeers())
            {
                Debug.WriteLine("No connections to send to.");
                return;
            }
            Debug.WriteLine(string.Format("Broadcasting {0}:{1} to all (count: {2})", ip.ToString(),
                port.ToString(), _peer.ConnectionsCount));
            NetOutgoingMessage msg = _peer.CreateMessage();
            msg.Write((int)Enums.MessageType.PeerInformation);
            byte[] addressBytes = ip.GetAddressBytes();
            msg.Write(addressBytes.Length);
            msg.Write(addressBytes);
            msg.Write(port);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>Send simple string to all peers </summary>
        public void SendObject(object datatosend)
        {
            if (!havePeers())
            {
                Debug.WriteLine("No connections to send to.");
                return;
            }
            
            NetOutgoingMessage msg = _peer.CreateMessage();
            msg.Write((byte)Enums.MessageType.Object);
            //ability to send diffrent types of data with ease
            var tranform = (CTransform)datatosend;
            msg.WriteCTransform(tranform);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.Unreliable, 0);
        }
        /// <summary>Send simple string to all peers </summary>
        public void SendMessage(string message)
        {
            if (!havePeers())
            {
                Debug.WriteLine("No connections to send to.");
                return;
            }
            NetOutgoingMessage msg = _peer.CreateMessage();

            msg.Write((byte)Enums.MessageType.StringMessage);
            msg.Write(message);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }
        /// <summary> Message loop to check type of message and handle it accordingly </summary>
        public void MessageLoop()
        {
            while ((_msg = _peer.ReadMessage()) != null)
            {
                switch (_msg.MessageType)
                {

                    case NetIncomingMessageType.DiscoveryRequest:
                        //Debug.WriteLine("ReceivePeersData DiscoveryRequest");
                        _peer.SendDiscoveryResponse(null, _msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        // just connect to first server discovered
                        //Debug.WriteLine("ReceivePeersData DiscoveryResponse CONNECT");
                        _peer.Connect(_msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        Debug.WriteLine("ReceivePeersData ConnectionApproval");
                        _msg.SenderConnection.Approve();
                        //broadcast this to all connected clients
                        SendPeerInfo(_msg.SenderEndPoint.Address, _msg.SenderEndPoint.Port);
                        break;
                    case NetIncomingMessageType.Data:
                        //another client sent us data
                        //Read TypeData First
                        Enums.MessageType mType = (Enums.MessageType) _msg.ReadByte();
           
                        if (mType == Enums.MessageType.StringMessage)
                        {
                            if (!_bot)
                                Game1.Inst.Scene.Raise("network_data_text", _msg.ReadString());
                        }
                        else if (mType == Enums.MessageType.PeerInformation)
                        {
                            int byteLenth = _msg.ReadInt32();
                            byte[] addressBytes = _msg.ReadBytes(byteLenth);
                            IPAddress ip = new IPAddress(addressBytes);
                            int port = _msg.ReadInt32();
                            //connect
                            IPEndPoint endPoint = new IPEndPoint(ip, port);
                            Debug.WriteLine("Data::PeerInfo::Detecting if we're connected");
                            if (_peer.GetConnection(endPoint) == null)
                            {//are we already connected?
                                //Don't try to connect to ourself!
                                if (_peer.Configuration.LocalAddress.GetHashCode() != endPoint.Address.GetHashCode()
                                    || _peer.Configuration.Port.GetHashCode() != endPoint.Port.GetHashCode())
                                {
                                    Debug.WriteLine(
                                        string.Format("Data::PeerInfo::Initiate new connection to: {0}:{1}",
                                            endPoint.Address.ToString(), endPoint.Port.ToString()));
                                    _peer.Connect(endPoint);
                                }
                            }
                        }
                        else if (mType == Enums.MessageType.Object)
                        {
                            //determine what type of data
                            //
                            var data = _msg.ReadCTransform();
                            Game1.Inst.Scene.Raise("network_data", data);
                        }
                        Console.WriteLine("END ReceivePeersData Data");
                        break;
                    case NetIncomingMessageType.UnconnectedData:
                        Debug.WriteLine("UnconnectedData: " + _msg.ReadString());
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Debug.WriteLine(_msg.ReadString());
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        Debug.WriteLine(_msg.ReadString());
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Debug.WriteLine(_msg.ReadString());
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Debug.WriteLine(_msg.ReadString());
                        break;
                    default:
                        Debug.WriteLine("ReceivePeersData Unknown type: " + _msg.MessageType.ToString());
                        try
                        {
                            Debug.WriteLine(_msg.SenderConnection);
                            if (_msg.SenderConnection.Status == NetConnectionStatus.Disconnected)
                            {
                                //Maybe try to reconnect
                            }
                            Debug.WriteLine(_msg.ReadString());
                        }
                        catch
                        {
                            Debug.WriteLine("Couldn't parse unknown to string.");
                        }
                        break;
                }
            }
        }

        public override void Update(float t, float dt)
        {
            MessageLoop();
            base.Update(t, dt);
        }
    }
}
