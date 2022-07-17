using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;

namespace Network.Strategies.Routing
{
    public sealed class RandomRoutingStrategy : RoutingStrategy
    {
        public static Dictionary<string, Property> GetProperties()
        {
            return RoutingStrategy.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()); ;
        }
        public RandomRoutingStrategy(Guid routerID, Guid networkID) :
            base(routerID, networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                
            })
        { }

        public RandomRoutingStrategy(Guid routerID, Guid networkID, Dictionary<string, Property> properties) :
            base(routerID, networkID, properties)
        { }
    }
}
