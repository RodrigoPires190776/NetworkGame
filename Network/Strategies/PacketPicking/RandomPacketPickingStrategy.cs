using Network.Components;
using System;

namespace Network.Strategies.PacketPicking
{
    public class RandomPacketPickingStrategy : IPacketPickingStrategy
    {
        public Packet NextPacket(Router router)
        {
            return router.PacketQueue.Count > 0 ? router.PacketQueue[new Random().Next(router.PacketQueue.Count)] : null;
        }
    }
}
