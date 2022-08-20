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
        protected static new Dictionary<string, Property> GetProperties(List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>> properties)
        {
            properties.AddRange(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.MinProbability, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 0.5m)
                    }),
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.MaxProbability, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0.5m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 1m),
                        new Tuple<string, object>(Property.INITIAL_VALUE, 1m)
                    })
            });
            return BaseStrategy.GetProperties(properties);
        }
        public RoutingStrategy(Guid routerID, Guid networkID, List<Tuple<string, PropertyType, List<Tuple<string, object>>>> properties) :
            base(networkID, properties)
        { 
            RouterID = routerID;
            Properties.Add(Property.MinProbability, new Property(Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 0.5m)
                    }));
            Properties.Add(Property.MaxProbability, new Property(Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0.5m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 1m),
                        new Tuple<string, object>(Property.INITIAL_VALUE, 1m)
                    }));
        }
        public RoutingStrategy(Guid routerID, Guid networkID, Dictionary<string, Property> properties) :
            base(networkID, properties)
        {
            RouterID = routerID;
        }

        public virtual void Initialize(Network network, Router router)
        {
            RoutingTable = new RoutingTable();
            RoutingTable.Initialize(network, router, (decimal)Properties[Property.MinProbability].Value, (decimal)Properties[Property.MaxProbability].Value);
        }
        public virtual Link NextLink(Router router, Packet packet)
        {
            return router.Links[RoutingTable.GetLink(packet.Destination)];
        }
        public virtual void Learn(Packet packet) { }
    }

    public class RoutingTable
    {
        private decimal MaxProbability = 1m;
        private decimal MinProbability = 0m;
        private ConcurrentDictionary<Guid, Dictionary<Guid, decimal>> Values { get; }
        private Dictionary<Guid, Dictionary<Guid, decimal>> OldValues { get; set; }
        public RoutingTable()
        {
            Values = new ConcurrentDictionary<Guid, Dictionary<Guid, decimal>>();
        }

        public virtual void Initialize(Network network, Router router, decimal minValue, decimal maxValue)
        {
            MaxProbability = maxValue;
            MinProbability = minValue;
            foreach(var routerID in network.RouterIDList)
            {
                if (routerID == router.ID) continue;
                
                var routerProbabilities = new Dictionary<Guid, decimal>();
                decimal initProb = 1m / router.Links.Keys.Count;

                foreach (var link in router.Links.Keys)
                {
                    routerProbabilities.Add(link, initProb);
                }

                Values.TryAdd(routerID, routerProbabilities);
            }   
        }

        public virtual void SetValues(Guid destinationNode, Dictionary<Guid, decimal> probabilities)
        {
            Values[destinationNode] = probabilities;
        }

        public virtual Guid GetLink(Guid router)
        {
            decimal random = (decimal) new Random().NextDouble();
            decimal sum = 0;
            Guid id;

            foreach (var i in Values[router].Keys.ToList())
            {
                id = i;
                sum += Values[router][id];
                if (sum > random) return id;
            }

            return id;
        }

        private decimal BalanceValue(Guid destinationID, Guid link, decimal value)
        {
            return value * (1 - Values[destinationID][link]);
        }

        public virtual void UpdateValue(Guid destinationID, Guid link, decimal value)
        {
            value = BalanceValue(destinationID, link, value);
            decimal originalValue = Values[destinationID][link];
            Values[destinationID][link] = Values[destinationID][link] + value;

            if (Values[destinationID][link] > MaxProbability) Values[destinationID][link] = MaxProbability;
            if (Values[destinationID][link] < MinProbability) Values[destinationID][link] = MinProbability;

            var delta = UpdateAllOtherValues(destinationID, link, Values[destinationID][link] - originalValue);

            Values[destinationID][link] = Values[destinationID][link] - delta;
        }

        private decimal UpdateAllOtherValues(Guid destinationID, Guid link, decimal delta)
        {
            decimal update;
            decimal originalValue;
            bool changed = true;
            var listProbability = Values[destinationID].Keys.ToList();
            listProbability.Remove(link);

            while (delta != 0 && changed && listProbability.Count > 0)
            {
                changed = false;
                update = delta / listProbability.Count;
                var listToRemove = new List<Guid>();
                foreach (var probability in listProbability)
                {
                    originalValue = Values[destinationID][probability];
                    Values[destinationID][probability] = Values[destinationID][probability] - update;

                    if (Values[destinationID][probability] > MaxProbability) Values[destinationID][probability] = MaxProbability;
                    if (Values[destinationID][probability] < MinProbability) Values[destinationID][probability] = MinProbability;

                    if (Values[destinationID][probability] != originalValue)
                    {
                        changed = true;
                        delta += Values[destinationID][probability] - originalValue;
                    }
                    else listToRemove.Add(probability);
                }

                foreach (var probability in listToRemove) listProbability.Remove(probability);
            }

            return delta;
        }

        public Dictionary<Guid, decimal> GetPercentageValues(Guid router)
        {
            var result = new Dictionary<Guid, decimal>();

            foreach(var id in Values[router].Keys.ToList())
            {
                result.Add(id, Values[router][id] * 100);
            }

            return result;
        }

        public decimal GetVariance()
        {
            if(OldValues == null)
            {
                OldValues = new Dictionary<Guid, Dictionary<Guid, decimal>>();
                UpdateOldValues();
                return 0m;
            }
            decimal averageVariance = 0m;

            foreach(var router in Values.Keys)
            {
                decimal localVariance = 0m;

                foreach(var link in Values[router].Keys)
                {
                    localVariance += Math.Abs(Values[router][link] - OldValues[router][link]);
                }
                averageVariance += localVariance / Values[router].Count;
            }

            UpdateOldValues();
            return averageVariance / Values.Count;
        }

        private void UpdateOldValues()
        {
            foreach(var router in Values.Keys)
            {
                var linkProbabilities = new Dictionary<Guid, decimal>();

                foreach(var probability in Values[router])
                {
                    linkProbabilities[probability.Key] = probability.Value;
                }

                OldValues[router] = linkProbabilities;
            }
        } 
    }
}
