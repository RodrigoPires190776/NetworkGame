using Network.Components;
using Network.UpdateNetwork;
using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;

namespace Network
{
    public class Network
    {
        public Dictionary<Guid, Router> Routers { get; private set; }
        public List<Link> Links { get; private set; }
        public Guid ID { get; private set; }

        public Network()
        {
            Routers = new();
            Links = new();
            ID = new Guid();
        }

        public void AddRouter(Router router)
        {
            Routers.Add(router.ID, router);
        }

        public void AddLink(Link link)
        {
            Links.Add(link);
            Routers[link.Routers.Item1].AddLink(link);
            Routers[link.Routers.Item2].AddLink(link);
        }

        public UpdatedState Step()
        {
            var state = new UpdatedState(ID);

            foreach (Link link in Links)
            {
                var reachedEnd = link.Step();

                state.UpdatedLinks.Add(new UpdateLink(link.ID, link.PackagesInTransit));

                foreach (Packet packet in reachedEnd)
                {
                    state.UpdatedPackets.Add(new UpdatePacket(packet.ID, packet.NumberOfSteps, packet.ReachedDestination));

                    if (!packet.ReachedDestination)
                    {
                        Routers[packet.CurrentRouter].AddPacket(packet);
                    }
                }
            }

            foreach(Router router in Routers.Values)
            {
                router.Step();

                state.UpdatedRouters.Add(new UpdateRouter(router.ID, router.PacketQueue.Count));
            }

            return state;
        }
    }
}
