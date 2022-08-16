using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;

namespace Network.RouteDiscovery
{
    public class BreadthFirstRouteDiscovery : BaseRouteDiscovery
    {
        public BreadthFirstRouteDiscovery(Network network) 
            : base(network)
        { }
        public static Dictionary<string, Property> GetProperties()
        {
            var properties = new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>();
            /*{
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.LearningWeight, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m)
                    })
            };*/

            var dictionaryProperties = BaseRouteDiscovery.GetProperties(properties);

            //dictionaryProperties[Property.LearningWeight].SetValue(1m);

            return dictionaryProperties;
        }

        public override void Discovery()
        {
            var allNodeDistances = new Dictionary<Guid, Dictionary<Guid, int>>();

            foreach(var node in Network.RouterIDList)
            {
                var nodeDistances = BreadthFirstSearch(node);
                allNodeDistances.Add(node, nodeDistances);
            }

            foreach(var node in Network.RouterIDList)
            {
                foreach (var nodeDestination in Network.RouterIDList)
                {
                    var costSum = 0m;
                    foreach (var link in Network.Routers[node].Links.Values)
                    {
                        costSum += allNodeDistances[GetOtherRouter(node, link)][nodeDestination];
                    }

                    var nodeLinkValue = new Dictionary<Guid, decimal>();
                    var valueSum = 0m;
                    foreach(var link in Network.Routers[node].Links.Values)
                    {
                        var otherNode = GetOtherRouter(node, link);
                        nodeLinkValue.Add(link.ID, costSum / allNodeDistances[GetOtherRouter(node, link)][nodeDestination]);
                        valueSum += nodeLinkValue[link.ID];
                    }

                    var probabilities = new Dictionary<Guid, decimal>();
                    foreach(var link in Network.Routers[node].Links.Keys)
                    {
                        probabilities.Add(link, nodeLinkValue[link] / valueSum);
                    }

                    Network.Routers[node].RoutingStrategy.RoutingTable.SetValues(nodeDestination, probabilities);
                }                
            }
        }

        private Dictionary<Guid, int> BreadthFirstSearch(Guid node)
        {
            var nodeDistances = new Dictionary<Guid, int>();
            var queue = new Queue<Guid>();
            var root = node;
            queue.Enqueue(root);
            nodeDistances.Add(root, 1);

            while (queue.Count > 0)
            {
                node = queue.Dequeue();
                var router = Network.Routers[node];

                foreach(var edge in router.Links.Values)
                {
                    var neighbour = GetOtherRouter(node, edge);
                    if (!nodeDistances.ContainsKey(neighbour))
                    {
                        nodeDistances.Add(neighbour, nodeDistances[node] + 1);
                        queue.Enqueue(neighbour);
                    }
                }
            } 

            return nodeDistances;
        }

        private Guid GetOtherRouter(Guid node, Link link)
        {
            return link.Routers.Item1 == node ? link.Routers.Item2 : link.Routers.Item1;
        }
    }
}
