using Network.Components;
using System;

namespace Network.Strategies.PacketCreation
{
    public class RandomPacketCreationStrategy : IPacketCreationStrategy
    {
        public Packet CreatePacket(Router router)
        {
            if (new Random().Next(2) == 1) return null;

            Guid dstID = router.ID;
            var network = NetworkMaster.GetInstance().GetNetwork(router.NetworkID);

            while (dstID == router.ID)
            {
                var destination = new Random().Next(network.Routers.Count);
                dstID = network.RouterIDList[destination];
            }
            
            return new Packet(router.ID, dstID, router.NetworkID);
        }
    }
}
