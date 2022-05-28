using Network.Strategies;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using System;
using System.Collections.Generic;

namespace Network.Components
{
    public class Router
    {
        public Coordinates Coordinates { get; }
        public Guid ID { get; }
        public Guid NetworkID { get; }
        public Dictionary<Guid, Link> Links { get; }
        public List<Packet> PacketQueue { get; }
        public RoutingStrategy RoutingStrategy { get; private set; }
        public PacketPickingStrategy PacketPickingStrategy { get; private set; }
        public PacketCreationStrategy PacketCreationStrategy { get; private set; }

        public Router(Guid networkID, Coordinates coordinates)
        {
            ID = Guid.NewGuid();
            Coordinates = coordinates;
            NetworkID = networkID;
            Links = new Dictionary<Guid, Link>();
            PacketQueue = new List<Packet>();
            RoutingStrategy = new RandomRoutingStrategy();
            PacketPickingStrategy = new RandomPacketPickingStrategy();
            PacketCreationStrategy = new RandomPacketCreationStrategy();
        }

        public void AddLink(Link link)
        {
            Links.Add(link.ID, link);
        }

        public void AddPacket(Packet packet)
        {
            PacketQueue.Add(packet);
        }
        
        public void SetStrategies(RoutingStrategy routing, PacketCreationStrategy packetCreation, PacketPickingStrategy packetPicking)
        {
            RoutingStrategy = routing ?? RoutingStrategy;
            RoutingStrategy.Initialize(NetworkMaster.GetInstance().GetNetwork(NetworkID), this);
            PacketCreationStrategy = packetCreation ?? PacketCreationStrategy;
            PacketPickingStrategy = packetPicking ?? PacketPickingStrategy;
        }

        public void Step()
        {
            var newPacket = PacketCreationStrategy.CreatePacket(this);
            if (newPacket != null) PacketQueue.Add(newPacket);
            if(PacketQueue.Count > 0)
            {
                var nextPacket = PacketPickingStrategy.NextPacket(this);
                RoutingStrategy.NextLink(this, nextPacket).Send(this, nextPacket);
                PacketQueue.Remove(nextPacket);
            }
        }
    }

    public class Coordinates
    {
        public double X { get; }
        public double Y { get; }
        public Coordinates(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
