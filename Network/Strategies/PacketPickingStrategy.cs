using Network.Components;
using System;
using System.Collections.Generic;
using static NetworkUtils.Property;

namespace Network.Strategies
{
    public abstract class PacketPickingStrategy : BaseStrategy
    {
        public PacketPickingStrategy(Guid networkID, List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) : 
            base(networkID, properties)
        { }
        public abstract (Packet, bool) NextPacket(Router router);
    }
}
