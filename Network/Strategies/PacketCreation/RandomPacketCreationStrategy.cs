using Network.Components;
using System;

namespace Network.Strategies.PacketCreation
{
    public class RandomPacketCreationStrategy : IPacketCreationStrategy
    {
        public Packet CreatePacket(Router router)
        {
            var destination = new Random().Next(NetworkMaster.GetInstance().GetNetwork(router.NetworkID).Routers.Count);
            return new Random().Next(2) == 1 ? new Packet(router.ID, destination, router.NetworkID) : null;
        }
    }
}
