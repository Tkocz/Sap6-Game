using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineName.Systems
{
    public class NetworkSystem : EcsSystem
    {
        private NetPeer _peer;
        private NetPeerConfiguration _config;
        private NetClient _client;
        private int _port = 50001;
        private NetIncomingMessage _msg;

        //fuggly remove
        private int textHeight = 320;

        /// <summary>Inits networkssystems configures settings for lidgrens networks framework.</summary>
        public override void Init()
        {
            _config = new NetPeerConfiguration("Sap6_Networking")
            {
                Port = _port,
                AcceptIncomingConnections = true
                
                
            };
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            _config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            _peer = new NetPeer(_config);
            _peer.Start();
            _peer.DiscoverLocalPeers(50002);

            Game1.Inst.Scene.OnEvent("SendToPeer", data => this.SendMessage((string)data));
       
            Debug.WriteLine("Listening on " + _config.Port.ToString());
            base.Init();
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
            msg.Write((int)MessageType.PeerInformation);
            byte[] addressBytes = ip.GetAddressBytes();
            msg.Write(addressBytes.Length);
            msg.Write(addressBytes);
            msg.Write(port);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }
        /// <summary>Send simple string to all peers </summary>
        public void SendObject(object message,Type type)
        {
            if (!havePeers())
            {
                Debug.WriteLine("No connections to send to.");
                return;
            }
            NetOutgoingMessage msg = _peer.CreateMessage();
            msg.Write((byte)MessageType.Object);
            msg.Write(type.FullName);
            msg.WriteAllProperties(message);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
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

            msg.Write((byte)MessageType.StringMessage);
            msg.Write(message);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }
        enum MessageType
        {
            StringMessage,
            PeerInformation,
            Object
        }
        /// <summary> Message loop to check type of message and handle it accordingly </summary>
        public void MessageLoop()
        {
            while ((_msg = _peer.ReadMessage()) != null)
            {
                switch (_msg.MessageType)
                {

                    case NetIncomingMessageType.DiscoveryRequest:
                        Debug.WriteLine("ReceivePeersData DiscoveryRequest");
                        _peer.SendDiscoveryResponse(null, _msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        // just connect to first server discovered
                        Debug.WriteLine("ReceivePeersData DiscoveryResponse CONNECT");
                        _peer.Connect(_msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        Debug.WriteLine("ReceivePeersData ConnectionApproval");
                        _msg.SenderConnection.Approve();
                        //broadcast this to all connected clients
                        //SendPeerInfo(msg.SenderEndPoint.Address, msg.SenderEndPoint.Port);
                        break;
                    case NetIncomingMessageType.Data:
                        //another client sent us data
                        Debug.WriteLine("BEGIN ReceivePeersData Data");
                        MessageType mType = (MessageType) _msg.ReadInt32();

                        if (mType == MessageType.StringMessage)
                        {
                            int eid = Game1.Inst.Scene.AddEntity();
                            Game1.Inst.Scene.AddComponent<C2DRenderable>(eid, new CText()
                            {
                                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034"),
                                format = _msg.ReadString(),
                                color = Color.White,
                                position = new Vector2(320, textHeight),
                                origin = Vector2.Zero// 
                            });
                            textHeight=textHeight+20;
                        }
                        else if (mType == MessageType.PeerInformation)
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
                        else if (mType == MessageType.Object)
                        {

                            string type = _msg.ReadString();
                            switch (type)
                            {
                                case : "TransForm";
                            }
                            byte[] addressBytes = _msg.ReadAllProperties()
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
                                //try to reconnect
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
