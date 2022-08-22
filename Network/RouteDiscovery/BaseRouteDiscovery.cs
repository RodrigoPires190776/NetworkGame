using NetworkUtils;
using System;
using System.Collections.Generic;

namespace Network.RouteDiscovery
{
    public abstract class BaseRouteDiscovery
    {
        protected Network Network;
        protected Dictionary<string, Property> Properties;
        public BaseRouteDiscovery(Network network)
        {
            Network = network;
        }

        public BaseRouteDiscovery(Network network, Dictionary<string, Property> properties)
        {
            Network = network;
            Properties = properties;
        }

        public virtual void Discovery()
        {

        }
        protected static Dictionary<string, Property> GetProperties(List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>> properties)
        {
            var dictionaryProperties = new Dictionary<string, Property>();

            foreach (var property in properties)
            {
                dictionaryProperties.Add(property.Item1, new Property(property.Item2, property.Item3));
            }

            return dictionaryProperties;
        }

        public enum RouteDiscoveryStrategies { None, BreadthFirstRouteDiscovery, DijkstraRouteDiscovery, BestRouteOnlyDiscovery };
        public static List<string> RouteDiscoveryList = new List<string> { "None", "BreadthFirstRouteDiscovery", "DijkstraRouteDiscovery", "BestRouteOnlyDiscovery" };

        public static RouteDiscoveryStrategies GetRouteDiscoveryEnum(string strategy)
        {
            switch (strategy)
            {
                case "None":
                    return RouteDiscoveryStrategies.None;
                case "BreadthFirstRouteDiscovery":
                    return RouteDiscoveryStrategies.BreadthFirstRouteDiscovery;
                case "DijkstraRouteDiscovery":
                    return RouteDiscoveryStrategies.DijkstraRouteDiscovery;
                case "BestRouteOnlyDiscovery":
                    return RouteDiscoveryStrategies.BestRouteOnlyDiscovery;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Dictionary<string, Property> GetRouteDiscoveryProperties(string strategy)
        {
            switch (strategy)
            {
                case "None":
                    return new Dictionary<string, Property>();
                case "BreadthFirstRouteDiscovery":
                    return BreadthFirstRouteDiscovery.GetProperties();
                case "DijkstraRouteDiscovery":
                    return DijkstraRouteDiscovery.GetProperties();
                case "BestRouteOnlyDiscovery":
                    return BestRouteOnlyDiscovery.GetProperties();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
