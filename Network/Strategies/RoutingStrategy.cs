using Network.Components;
using Network.UpdateNetwork.UpdateObjects;
using NetworkUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static NetworkUtils.Property;

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
            Properties.Add("", new Property(PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(DECIMAL_MIN, 0m)
                    }));
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
        public virtual decimal Learn(Packet packet) { return 0; }
    }

    public class RoutingTable
    {
        private decimal MaxValueMultipler = 10m;
        private ConcurrentDictionary<Guid, Dictionary<Guid, decimal>> Values { get; }
        private ConcurrentDictionary<Guid, decimal> Sum;
        private object _valuesLock = new object();
        public RoutingTable()
        {
            Values = new ConcurrentDictionary<Guid, Dictionary<Guid, decimal>>();
            Sum = new ConcurrentDictionary<Guid, decimal>();
        }

        public virtual void Initialize(Network network, Router router)
        {
            foreach(var routerID in network.RouterIDList)
            {
                if (routerID == router.ID) continue;

                Sum.TryAdd(routerID, 0);
                
                var routerProbabilities = new Dictionary<Guid, decimal>();

                foreach (var link in router.Links.Keys)
                {
                    routerProbabilities.Add(link, 1);
                    Sum[routerID] = Sum[routerID] + 1;
                }

                Values.TryAdd(routerID, routerProbabilities);
            }   
        }

        public virtual Guid GetLink(Guid router)
        {
            int random = new Random().Next((int)Math.Ceiling(Sum[router] * 100));
            int sum = 0;
            Guid id;

            foreach (var i in Values[router].Keys.ToList())
            {
                id = i;
                sum += (int)Math.Floor(Values[router][id] * 100);
                if (sum > random) return id;
            }


            return id;
        }

        public virtual decimal UpdateValue(Guid destinationID, Guid link, decimal value)
        {
            decimal initialSum = Sum[destinationID];
            Sum[destinationID] = Sum[destinationID] + value;
            Values[destinationID][link] = Values[destinationID][link] + value;

            if (Sum[destinationID] > Values[destinationID].Count * MaxValueMultipler)
            {
                var equalizer = Sum[destinationID] / (Values[destinationID].Count * MaxValueMultipler);
                foreach (var linkProb in Values[destinationID].Keys.ToList())
                {
                    Values[destinationID][linkProb] = Values[destinationID][linkProb] / equalizer;
                }
                Sum[destinationID] = Sum[destinationID] / equalizer;
            }

            return Sum[destinationID] / initialSum;
        }

        public Dictionary<Guid, decimal> GetPercentageValues(Guid router)
        {
            var result = new Dictionary<Guid, decimal>();

            foreach(var id in Values[router].Keys.ToList())
            {
                result.Add(id, (decimal)Values[router][id] * 100 / Sum[router]);
            }

            return result;
        }
    }
}
