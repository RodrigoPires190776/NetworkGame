using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using static NetworkUtils.Property;

namespace Network.Strategies
{
    public abstract class PacketCreationStrategy : BaseStrategy
    {
        protected static Dictionary<string, Property> GetProperties(List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>> properties)
        {
            return BaseStrategy.GetProperties(properties);
        }
        public PacketCreationStrategy(Guid networkID, List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) :
            base(networkID, properties)
        { }

        public abstract Packet CreatePacket(Router router);
    }
}
