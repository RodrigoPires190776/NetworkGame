using Network.Components;
using System;
using System.Collections.Generic;
using static Network.Strategies.Property;

namespace Network.Strategies
{
    public abstract class RoutingStrategy : BaseStrategy
    {
        public RoutingTable RoutingTable { get; protected set; }
        public RoutingStrategy(List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) :
            base(properties)
        { }
        public virtual void Initialize(Network network, Router router)
        {
            RoutingTable = new RoutingTable();
            RoutingTable.Initialize(network, router);
        }
        public abstract Link NextLink(Router router, Packet packet);
        public abstract void Learn(object parameters);
    }

    public class RoutingTable
    {
        private Dictionary<Guid,Dictionary<Guid, int>> Values { get; }
        private Dictionary<Guid, int> Sum;
        public RoutingTable()
        {
            Values = new Dictionary<Guid, Dictionary<Guid, int>>();
            Sum = new Dictionary<Guid, int>();
        }

        public virtual void Initialize(Network network, Router router)
        {
            foreach(var routerID in network.RouterIDList)
            {
                if (routerID == router.ID) continue;

                Sum.Add(routerID, 0);
                
                var routerProbabilities = new Dictionary<Guid, int>();

                foreach (var link in router.Links.Keys)
                {
                    routerProbabilities.Add(link, 1);
                    Sum[routerID] = Sum[routerID] + 1;
                }

                Values.Add(routerID, routerProbabilities);
            }   
        }

        public virtual Guid GetLink(Guid router)
        {
            int random = new Random().Next(Sum[router]);
            int sum = -1;
            Guid id;

            foreach(var i in Values[router].Keys)
            {
                id = i;
                sum += Values[router][id];
                if (sum >= random) return id; 
            }

            return id;
        }

        public virtual void Learn(object parameters)
        {

        }

        public Dictionary<Guid, decimal> GetPercentageValues(Guid router)
        {
            var result = new Dictionary<Guid, decimal>();

            foreach (var id in Values[router].Keys)
            {
                result.Add(id, (decimal)Values[router][id] / Sum[router]);
            }

            return result;
        }
    }
}
