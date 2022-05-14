using Network.Components;
using System;

namespace Network.Strategies.Routing
{
    public class RandomRoutingStrategy : IRoutingStrategy
    {
        public Link NextLink(Router router, Packet packet)
        {
            return router.Links[new Random().Next(router.Links.Count)];
        }
    }
}
