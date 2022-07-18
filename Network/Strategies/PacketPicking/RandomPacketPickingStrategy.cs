using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;

namespace Network.Strategies.PacketPicking
{
    public sealed class RandomPacketPickingStrategy : PacketPickingStrategy
    {
        public static Dictionary<string, Property> GetProperties()
        {
            return PacketPickingStrategy.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()); ;
        }
        public RandomPacketPickingStrategy(Guid networkID) :
            base(networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                
            })
        { }

        public RandomPacketPickingStrategy(Guid networkID, Dictionary<string, Property> properties) :
            base(networkID, properties)
        { }
        public override (Packet, bool) NextPacket(Router router)
        {
            return router.PacketQueue.Count > 0 ? (router.PacketQueue[new Random().Next(router.PacketQueue.Count)], true) : (null, true);
        }
    }
}
