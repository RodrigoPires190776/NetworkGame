using Network.Components;
using System;
using System.Collections.Generic;

namespace Network.Strategies.Routing
{
    public sealed class RandomRoutingStrategy : RoutingStrategy
    {
        public RandomRoutingStrategy() :
            base(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                
            })
        { }
        public override Link NextLink(Router router, Packet packet)
        {
            return router.Links[RoutingTable.GetLink(packet.Destination)];
        }

        public override void Learn(object parameters)
        {
            
        }
    }
}
