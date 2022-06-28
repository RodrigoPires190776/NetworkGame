using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;

namespace Network.Strategies.PacketPicking
{
    public sealed class RandomPacketPickingStrategy : PacketPickingStrategy
    {
        public RandomPacketPickingStrategy(Guid networkID) :
            base(networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                
            })
        { }
        public override (Packet, bool) NextPacket(Router router)
        {
            return router.PacketQueue.Count > 0 ? (router.PacketQueue[new Random().Next(router.PacketQueue.Count)], true) : (null, true);
        }
    }
}
