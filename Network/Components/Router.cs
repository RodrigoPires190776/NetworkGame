using Network.Strategies;
using System;
using System.Collections.Generic;

namespace Network.Components
{
    public class Router
    {
        public Coordinates Coordinates { get; }
        public Guid ID { get; }
        public Guid NetworkID { get; }
        public List<Link> Links { get; }
        public List<Packet> PacketQueue { get; }
        private IRoutingStrategy RoutingStrategy { get; }
        private IPacketPickingStrategy PacketPickingStrategy { get; }
        private IPacketCreationStrategy PacketCreationStrategy { get; }

        public Router(Guid networkID, Coordinates coordinates, IRoutingStrategy routingStrategy, IPacketPickingStrategy packetPickingStrategy, IPacketCreationStrategy packetCreationStrategy)
        {
            ID = new Guid();
            Coordinates = coordinates;
            NetworkID = networkID;
            Links = new List<Link>();
            PacketQueue = new List<Packet>();
            RoutingStrategy = routingStrategy;
            PacketPickingStrategy = packetPickingStrategy;
            PacketCreationStrategy = packetCreationStrategy;
        }

        public void AddLink(Link link)
        {
            Links.Add(link);
        }

        public void AddPacket(Packet packet)
        {
            PacketQueue.Add(packet);
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
