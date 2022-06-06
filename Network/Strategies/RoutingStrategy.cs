using Network.Components;
using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;
using static Network.Strategies.Property;

namespace Network.Strategies
{
    public abstract class RoutingStrategy : BaseStrategy
    {
        public RoutingTable RoutingTable { get; protected set; }
        public Guid RouterID { get; }
        public RoutingStrategy(Guid routerID, Guid networkID, List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) :
            base(networkID, properties)
        { 
            RouterID = routerID; 
        }
        public virtual void Initialize(Network network, Router router)
        {
            RoutingTable = new RoutingTable();
            RoutingTable.Initialize(network, router);
        }
        public virtual Link NextLink(Router router, Packet packet)
        {
            return router.Links[RoutingTable.GetLink(packet.Destination)];
        }
        public virtual void Learn(Packet packet) { }
    }

    public class RoutingTable
    {
        private Dictionary<Guid,Dictionary<Guid, decimal>> Values { get; }
        private Dictionary<Guid, decimal> Sum;
        public RoutingTable()
        {
            Values = new Dictionary<Guid, Dictionary<Guid, decimal>>();
            Sum = new Dictionary<Guid, decimal>();
        }

        public virtual void Initialize(Network network, Router router)
        {
            foreach(var routerID in network.RouterIDList)
            {
                if (routerID == router.ID) continue;

                Sum.Add(routerID, 0);
                
                var routerProbabilities = new Dictionary<Guid, decimal>();

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
            int random = new Random().Next((int)Math.Ceiling(Sum[router] * 100));
            int sum = 0;
            Guid id;

            foreach(var i in Values[router].Keys)
            {
                id = i;
                sum +=  (int)Math.Floor(Values[router][id] * 100);
                if (sum > random) return id; 
            }

            return id;
        }

        public virtual void UpdateValue(Guid destinationID, Guid link, decimal value)
        {
            Sum[destinationID] = Sum[destinationID] + value;
            Values[destinationID][link] = Values[destinationID][link] + value;
        }

        public Dictionary<Guid, decimal> GetPercentageValues(Guid router)
        {
            var result = new Dictionary<Guid, decimal>();

            foreach (var id in Values[router].Keys)
            {
                result.Add(id, (decimal)Values[router][id] * 100 / Sum[router]);
            }

            return result;
        }
    }
}
