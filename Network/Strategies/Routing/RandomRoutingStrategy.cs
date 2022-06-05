using Network.Components;
using System;
using System.Collections.Generic;

namespace Network.Strategies.Routing
{
    public sealed class RandomRoutingStrategy : RoutingStrategy
    {
        public RandomRoutingStrategy(Guid routerID, Guid networkID) :
            base(routerID, networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                
            })
        { }
    }
}
