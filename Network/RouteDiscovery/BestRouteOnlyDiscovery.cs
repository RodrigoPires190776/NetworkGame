using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Network.RouteDiscovery
{
    public class BestRouteOnlyDiscovery : BaseRouteDiscovery
    {
        public BestRouteOnlyDiscovery(Network network, Dictionary<string, Property> properties)
            : base(network, properties)
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

            foreach (var node in Network.RouterIDList)
            {
                var nodeDistances = Dijkstra(node);
                allNodeDistances.Add(node, nodeDistances);
            }

            foreach (var node in Network.RouterIDList)
            {
                foreach (var nodeDestination in Network.RouterIDList)
                {
                    if (node == nodeDestination) continue;
                    var costSum = 0m;
                    foreach (var link in Network.Routers[node].Links.Values)
                    {
                        costSum += allNodeDistances[GetOtherRouter(node, link)][nodeDestination] + link.LinkLength;
                    }

                    var lowestCost = decimal.MaxValue;
                    var lowestCostLink = Guid.Empty;
                    var probabilities = new Dictionary<Guid, decimal>();
                    foreach (var link in Network.Routers[node].Links.Values)
                    {
                        probabilities.Add(link.ID, 0);
                        var cost = allNodeDistances[GetOtherRouter(node, link)][nodeDestination] + link.LinkLength;
                        if(cost < lowestCost)
                        {
                            lowestCost = cost;
                            lowestCostLink = link.ID;
                        }
                    }

                    probabilities[lowestCostLink] = 1m;
                    Network.Routers[node].RoutingStrategy.RoutingTable.SetValues(nodeDestination, probabilities);
                }
            }
        }

        private Dictionary<Guid, int> Dijkstra(Guid node)
        {
            var nodeDistances = new Dictionary<Guid, int>();
            Guid nodeMin;
            int nodeMinValue;
            nodeDistances.Add(node, 1);
            var list = new List<Guid>() { node };

            while (list.Count > 0)
            {
                nodeMin = list[0];
                nodeMinValue = nodeDistances.ContainsKey(list[0]) ? nodeDistances[list[0]] : int.MaxValue;
                foreach (var n in list)
                {
                    if (nodeDistances.ContainsKey(n) && nodeDistances[n] < nodeMinValue)
                    {
                        nodeMin = n;
                        nodeMinValue = nodeDistances[n];
                    }
                }
                node = nodeMin;
                list.Remove(node);

                var router = Network.Routers[node];

                foreach (var edge in router.Links.Values)
                {
                    var neighbour = GetOtherRouter(node, edge);
                    var cost = nodeDistances[node] + edge.LinkLength;

                    if (!nodeDistances.ContainsKey(neighbour))
                    {
                        nodeDistances.Add(neighbour, cost);
                        list.Add(neighbour);
                    }
                    else if (cost < nodeDistances[neighbour])
                    {
                        nodeDistances[neighbour] = cost;
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
