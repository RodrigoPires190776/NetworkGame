using Network.Components;
using System.Collections.Generic;

namespace Network
{
    public class Network
    {
        public Dictionary<int, Router> Routers { get; private set; }
        public List<Link> Links { get; private set; }

        public Network()
        {
            Routers = new();
            Links = new();
        }

        public void Start()
        {
            
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

        public void Step()
        {
            foreach (Link link in Links)
            {
                var reachedEnd = link.Step();

                foreach (Packet packet in reachedEnd)
                {
                    if (!packet.ReachedDestination)
                    {
                        Routers[packet.CurrentRouter].AddPacket(packet);
                    }
                }
            }

            foreach(Router router in Routers.Values)
            {
                router.Step();
            }
        }
    }
}
