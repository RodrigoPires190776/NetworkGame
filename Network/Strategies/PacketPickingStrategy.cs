using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using static NetworkUtils.Property;

namespace Network.Strategies
{
    public abstract class PacketPickingStrategy : BaseStrategy
    {
        protected static new Dictionary<string, Property> GetProperties(List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>> properties)
        {
            return BaseStrategy.GetProperties(properties);
        }
        public PacketPickingStrategy(Guid networkID, List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) : 
            base(networkID, properties)
        { }

        public PacketPickingStrategy(Guid networkID, Dictionary<string, Property> properties) :
            base(networkID, properties)
        { }
        public abstract (Packet, bool) NextPacket(Router router);
        public abstract void AddPacket(Packet packet);
    }
}
