using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Utils;
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
        private int _localport = 50001;
        private int _searchport = 50001;
        private NetIncomingMessage _msg;
        private bool _bot = false;
        private Thread _scanThread;
        public NetworkSystem()
        {
        }
        public NetworkSystem(int port)
        {
            _localport = port;
        }
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
                Game1.Inst.Scene.OnEvent("send_to_peer", data => this.SendObject((string)data , "metadata"));
                Game1.Inst.Scene.OnEvent("search_for_peers", data => _peer.DiscoverLocalPeers(_searchport));
                 Game1.Inst.Scene.OnEvent("search_for_peers", data => _peer.DiscoverLocalPeers(_searchport));
            }

            _scanThread = new Thread(ScanForNewPeers);
            _scanThread.Start();
            base.Init();
        }
        /// <summary>Periodically scan for new peers</summary>
        private void ScanForNewPeers()
        {
            while (true)
            {
                _peer.DiscoverLocalPeers(_searchport);
                Thread.Sleep(1000);
            }   
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
                if (counter % 2 == 0 && counter != 0)
                {
                    counter++;
                    
                    //SendObject(counter + "hej from bot","chatmessage");
                }
            }
        }
        /// <summary>Checks if have peers, so its possible to send stuff</summary>
        private bool havePeers()
        {
            if(_peer.Connections != null && _peer.Connections.Count > 0)
                return true;

            _peer.DiscoverLocalPeers(_searchport);
            return false;
        }
        /// <summary>Send information about newly connected peer to all other peers for faster discovery </summary>
        public void SendPeerInfo(IPAddress ip, int port)
        {
            if (!havePeers())
            {
                //Debug.WriteLine("No connections to send to.");
                return;
            }
            Debug.WriteLine(string.Format("Broadcasting {0}:{1} to all (count: {2})", ip.ToString(),
                port.ToString(), _peer.ConnectionsCount));
            NetOutgoingMessage msg = _peer.CreateMessage();
            msg.Write((byte)Enums.MessageType.PeerInformation);
            byte[] addressBytes = ip.GetAddressBytes();
            msg.Write(addressBytes.Length);
            msg.Write(addressBytes);
            msg.Write(port);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendCObject(CTransform cTransform, CBody cBody, int id,string modelfilename)
        {
            if (!havePeers())
            {
                //Debug.WriteLine("No connections to send to.");
                return;
            }
            NetOutgoingMessage msg = _peer.CreateMessage();
            msg.Write((byte)Enums.MessageType.Entity);
            msg.WriteEntity(id, cBody,cTransform, modelfilename);
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.Unreliable, 0);
        }

        /// <summary>Send simple string to all peers </summary>
        public void SendObject(object datatosend, string metadata)
        {
            if (!havePeers())
            {
                Debug.WriteLine("No connections to send to.");
                return;
            }
            
            Enums.MessageType type = Enums.MessageType.Unknown;
            Enum.TryParse(datatosend.GetType().Name, out type);
            if(type== Enums.MessageType.Unknown)
                return;

            NetOutgoingMessage msg = _peer.CreateMessage();
            switch (type)
            {
                case Enums.MessageType.CBody:
                    var dataCbody = (CBody)datatosend;
                    msg.Write((byte)type);
                    msg.Write(metadata);
                    msg.WriteCBody(dataCbody);
                    break;
                case Enums.MessageType.CTransform:
                    var dataTransform = (CTransform)datatosend;
                    msg.Write((byte)type);
                    msg.Write(metadata);
                    msg.WriteCTransform(dataTransform);
                    break;
                case Enums.MessageType.Vector3:
                    var datavector = (Vector3)datatosend;
                    msg.Write((byte)type);
                    msg.Write(metadata);
                    msg.WriteUnitVector3(datavector, 1);
                    break;
                case Enums.MessageType.Int:
                    int dataint = (int)datatosend;
                    msg.Write((byte)type);
                    msg.Write(metadata);
                    msg.Write(dataint);
                    break;
                case Enums.MessageType.String:
                    var datastring = (string)datatosend;
                    msg.Write((byte)type);
                    msg.Write(metadata);
                    msg.Write(datastring);
                    break;
                default:
                    Debug.WriteLine("unknownType");
                    break;

            }
            //ability to send diffrent types of data with ease
            _peer.SendMessage(msg, _peer.Connections, NetDeliveryMethod.Unreliable, 0);
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
                        Game1.Inst.Scene.Raise("peer_data", _peer.Connections.Select(x => x.RemoteEndPoint.Address.ToString()).ToList());
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
           
                        if (mType == Enums.MessageType.String)
                        {
                            if (!_bot)
                            {
                                var metadata = _msg.ReadString();
                                Game1.Inst.Scene.Raise("network_data_text", _msg.ReadString());                               
                            }
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
                        else if (mType == Enums.MessageType.Entity)
                        {
                            var cbody = new CBody();
                            var ctransform = new CTransform();
                            string modelname = "";
                            var id  =_msg.ReadEntity(ref cbody,  ref ctransform,  ref modelname);
                            addOrUpdatCObjects(id, cbody, ctransform, modelname);
                            //Game1.Inst.Scene.Raise("network_data", data);
                        }
                        else if (mType == Enums.MessageType.CTransform)
                        {
                            var metadata = _msg.ReadString();
                            var data = _msg.ReadCTransform();
                            //Game1.Inst.Scene.Raise("network_data", data);
                        }
                        else if (mType == Enums.MessageType.Vector3)
                        {
                            var metadata = _msg.ReadString();
                            var data = _msg.ReadCTransform();
                            //Game1.Inst.Scene.Raise("network_data", data);
                        }
                        else if (mType == Enums.MessageType.Int)
                        {
                            var metadata = _msg.ReadString();
                            var data = _msg.ReadInt32();
                            //Game1.Inst.Scene.Raise("network_data", data);
                        }
                        //Console.WriteLine("END ReceivePeersData Data");
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

        private void syncObjects()
        {
            foreach (var key in Game1.Inst.Scene.GetComponents<CSyncObject>().Keys)
            {
                var model = (CImportedModel)Game1.Inst.Scene.GetComponentFromEntity<C3DRenderable>(key);
                var ctransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(key);
                var cbody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(key);
                SendCObject(ctransform, cbody, key, model.fileName);
            }
        }

        private void addOrUpdatCObjects(int id, CBody cbody, CTransform ctransform,string modelname)
        {
            //Add entity 
            if (!Game1.Inst.Scene.EntityHasComponent<CTransform>(id))
            {
                //calculate BoundindBox since we have the data do this
                cbody.Aabb = new BoundingBox(-cbody.Radius * Vector3.One, cbody.Radius * Vector3.One);
                Game1.Inst.Scene.AddComponent(id, cbody);
                Game1.Inst.Scene.AddComponent(id, ctransform);
                Game1.Inst.Scene.AddComponent<C3DRenderable>(id, new CImportedModel
                {
                    model = Game1.Inst.Content.Load<Model>(modelname),
                    fileName = modelname
                });
            }
            else
            {
                //Update postition
                var oldctransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(id);
                oldctransform.Frame = ctransform.Frame;
                oldctransform.Rotation = ctransform.Rotation;
                oldctransform.Position = ctransform.Position;
                oldctransform.Scale = ctransform.Scale;
            }   
        }
        public override void Cleanup()
        {
            _scanThread.Abort();
            _peer.Shutdown("Shutting Down");
            base.Cleanup();
        }
        private const float updateInterval  = 0.1f;
        private float reamaingTime = 0;
        public override void Update(float t, float dt)
        {
            reamaingTime += dt;
            if (reamaingTime> updateInterval)
            {
                syncObjects();
                reamaingTime = 0;
            }
          
            MessageLoop();
            base.Update(t, dt);
        }
    }
}
