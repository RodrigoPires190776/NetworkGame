using NetworkUtils;
using System;
using System.Collections.Generic;

namespace Network.RouteDiscovery
{
    public class NoneRouteDiscovery : BaseRouteDiscovery
    {
        public NoneRouteDiscovery(Network network)
            : base(network)
        { }
        public static Dictionary<string, Property> GetProperties()
        {
            var properties = new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>();

            var dictionaryProperties = BaseRouteDiscovery.GetProperties(properties);

            return dictionaryProperties;
        }
    }
}
