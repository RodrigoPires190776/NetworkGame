using Network.Components;
using System;
using System.Collections.Generic;
using static Network.Strategies.Property;

namespace Network.Strategies
{
    public abstract class PacketPickingStrategy : BaseStrategy
    {
        public PacketPickingStrategy(List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) :
            base(properties)
        { }
        public abstract Packet NextPacket(Router router);
    }
}
