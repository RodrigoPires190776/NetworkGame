using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Network.Strategies.PacketPicking
{
    public sealed class FIFOPacketPickingStrategy : PacketPickingStrategy
    {
        private Queue<Packet> PacketQueue { get; }
        public static Dictionary<string, Property> GetProperties()
        {
            return PacketPickingStrategy.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()); ;
        }
        public FIFOPacketPickingStrategy(Guid networkID) :
            base(networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            { })
        {
            PacketQueue = new Queue<Packet>();
        }

        public FIFOPacketPickingStrategy(Guid networkID, Dictionary<string, Property> properties) :
            base(networkID, properties)
        {
            PacketQueue = new Queue<Packet>();
        }
        public override (Packet, bool) NextPacket(Router router)
        {
            Packet packet = null;
            while(PacketQueue.Count > 0 && packet == null)
            {
                packet = PacketQueue.Dequeue();
                if (!router.PacketQueue.Contains(packet)) packet = null;
            }
            return (packet, true);
        }

        public override void AddPacket(Packet packet)
        {
            PacketQueue.Enqueue(packet);
        }
    }
}
