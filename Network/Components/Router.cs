using Network.Strategies;
using System.Collections.Generic;

namespace Network.Components
{
    public class Router
    {
        public int ID { get; }
        public List<Link> Links { get; }
        public List<Packet> PacketQueue { get; }
        private IRoutingStrategy RoutingStrategy { get; }
        private IPacketPickingStrategy PacketPickingStrategy { get; }
        private IPacketCreationStrategy PacketCreationStrategy { get; }

        public Router(int id, IRoutingStrategy routingStrategy, IPacketPickingStrategy packetPickingStrategy, IPacketCreationStrategy packetCreationStrategy)
        {
            ID = id;
            Links = new();
            PacketQueue = new();
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
}
