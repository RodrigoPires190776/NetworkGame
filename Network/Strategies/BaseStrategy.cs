using NetworkUtils;
using System;
using System.Collections.Generic;
using static NetworkUtils.Property;

namespace Network.Strategies
{
    public abstract class BaseStrategy
    {
        public Guid NetworkID { get; }
        public Dictionary<string, Property> Properties { get; }

        public BaseStrategy(Guid networkID, List<Tuple<string , PropertyType, List<Tuple<string, object>>>> properties)
        {
            NetworkID = networkID;
            Properties = new Dictionary<string, Property>();

            foreach(var property in properties)
            {
                Properties.Add(property.Item1, new Property(property.Item2, property.Item3));
            }
        }
    }
}
