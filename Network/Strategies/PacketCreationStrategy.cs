using Network.Components;
using System;
using System.Collections.Generic;
using static Network.Strategies.Property;

namespace Network.Strategies
{
    public abstract class PacketCreationStrategy : BaseStrategy
    {
        public PacketCreationStrategy(List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) :
            base(properties)
        { }

        public abstract Packet CreatePacket(Router router);
    }
}
