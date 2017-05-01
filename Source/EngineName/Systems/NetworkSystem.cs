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
        private int port = 50001;
        NetIncomingMessage _msg;
        private int textHeight = 320;
        public override void Init()
        {
            _config = new NetPeerConfiguration("Sap6");
            _config.Port = port;
            _config.AcceptIncomingConnections = true;
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            _config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            //Setup Connection First check if server is being hosted
            _peer = new NetPeer(_config);
            _peer.Start();
            _peer.DiscoverLocalPeers(port);
            Console.WriteLine("listening on " + _config.Port.ToString());
            base.Init();
        }
        public void SendMessage(string message)
        {
            if (_peer.Connections == null || _peer.Connections.Count == 0)
            {
                Console.WriteLine("No connections to send to.");
                return;
            }
            NetOutgoingMessage msg = _peer.CreateMessage();
            msg.Write((int)MessageType.StringMessage);
            msg.Write(message);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }
        enum MessageType
        {
            StringMessage,
            PeerInformation
        }

        public void MessageLoop()
        {
            NetIncomingMessage msg;
            while ((msg = _peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {

                    case NetIncomingMessageType.DiscoveryRequest:
                        Debug.WriteLine("ReceivePeersData DiscoveryRequest");
                        _peer.SendDiscoveryResponse(null, msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        // just connect to first server discovered
                        Debug.WriteLine("ReceivePeersData DiscoveryResponse CONNECT");
                        _peer.Connect(msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        Debug.WriteLine("ReceivePeersData ConnectionApproval");
                        msg.SenderConnection.Approve();
                        //broadcast this to all connected clients
                        //netManager.SendPeerInfo(msg.SenderEndPoint.Address, msg.SenderEndPoint.Port);
                        break;
                    case NetIncomingMessageType.Data:
                        //another client sent us data
                        Debug.WriteLine("BEGIN ReceivePeersData Data");
                        MessageType mType = (MessageType) msg.ReadInt32();
                        if (mType == MessageType.StringMessage)
                        {
                            int eid = Game1.Inst.Scene.AddEntity();
                            Game1.Inst.Scene.AddComponent<C2DRenderable>(eid, new CText()
                            {
                                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034"),
                                format = msg.ReadString(),
                                color = Color.White,
                                position = new Vector2(320, textHeight),
                                origin = Vector2.Zero// Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034").MeasureString("Sap my Low-Poly Game") / 2
                            });
                            textHeight=textHeight+20;
                        }
                        else if (mType == MessageType.PeerInformation)
                        {
                            int byteLenth = msg.ReadInt32();
                            byte[] addressBytes = msg.ReadBytes(byteLenth);
                            IPAddress ip = new IPAddress(addressBytes);
                            int port = msg.ReadInt32();
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
                        Console.WriteLine("END ReceivePeersData Data");
                        break;
                    case NetIncomingMessageType.UnconnectedData:
                        string orphanData = msg.ReadString();
                        Debug.WriteLine("UnconnectedData: " + orphanData);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Debug.WriteLine(msg.ReadString());
                        break;
                    default:
                        Debug.WriteLine("ReceivePeersData Unknown type: " + msg.MessageType.ToString());
                        try
                        {
                            Debug.WriteLine(msg.SenderConnection);
                            if (msg.SenderConnection.Status == NetConnectionStatus.Disconnected)
                            {
                                
                            }
                            Debug.WriteLine(msg.ReadString());
                        }
                        catch
                        {
                            Debug.WriteLine("Couldn't parse unknown to string.");
                        }
                        break;
                }
                // process message here
            }
        }

        public override void Update(float t, float dt)
        {
            foreach (var netComp in Game1.Inst.Scene.GetComponents<NetworkComponent>())
            {
                var mescomp = (NetworkComponent)netComp.Value;
                if (mescomp.StringMessage == null)
                    continue;

                SendMessage(mescomp.StringMessage);
                mescomp.StringMessage = null;
            }
           

            MessageLoop();
            base.Update(t, dt);
        }
    }
}
