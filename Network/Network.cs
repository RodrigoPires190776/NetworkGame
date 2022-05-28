using Network.Components;
using Network.Strategies;
using Network.UpdateNetwork;
using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;

namespace Network
{
    public class Network
    {
        public string Name { get; set; }
        public Dictionary<Guid, Router> Routers { get; private set; }
        public List<Guid> RouterIDList { get; private set; }
        public List<Link> Links { get; private set; }
        public Guid ID { get; private set; }

        public Network()
        {
            Routers = new Dictionary<Guid, Router>();
            RouterIDList = new List<Guid>();
            Links = new List<Link>();
            ID = Guid.NewGuid();
        }

        public void AddRouter(Router router)
        {
            Routers.Add(router.ID, router);
            RouterIDList.Add(router.ID);
        }

        public void AddLink(Link link)
        {
            Links.Add(link);
            Routers[link.Routers.Item1].AddLink(link);
            Routers[link.Routers.Item2].AddLink(link);
        }

        public void SetStrategies(Guid routerID, RoutingStrategy routing, PacketCreationStrategy packetCreation, PacketPickingStrategy packetPicking)
        {
            Routers[routerID].SetStrategies(routing, packetCreation, packetPicking);
        }

        public UpdatedState Step()
        {
            var state = new UpdatedState(ID);

            foreach (Link link in Links)
            {
                var reachedEnd = link.Step();

                state.UpdatedLinks.Add(link.ID, new UpdateLink(link.ID, link.PackagesInTransit));

                foreach (Packet packet in reachedEnd)
                {
                    state.UpdatedPackets.Add(packet.ID, new UpdatePacket(packet.ID, packet.NumberOfSteps, packet.ReachedDestination));

                    if (!packet.ReachedDestination)
                    {
                        Routers[packet.CurrentRouter].AddPacket(packet);
                    }
                }
            }

            foreach(Router router in Routers.Values)
            {
                router.Step();

                state.UpdatedRouters.Add(router.ID, new UpdateRouter(router.ID, router.PacketQueue.Count));
            }

            return state;
        }
    }
}
