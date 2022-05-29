using Network.Components;
using System;
using System.Collections.Generic;

namespace Network.Strategies.PacketPicking
{
    public sealed class RandomPacketPickingStrategy : PacketPickingStrategy
    {
        public RandomPacketPickingStrategy() :
            base(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                
            })
        { }
        public override Packet NextPacket(Router router)
        {
            return router.PacketQueue.Count > 0 ? router.PacketQueue[new Random().Next(router.PacketQueue.Count)] : null;
        }
    }
}
