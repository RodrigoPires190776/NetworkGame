using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
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

        protected static Dictionary<string, Property> GetProperties(List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>> properties)
        {
            var dictionaryProperties = new Dictionary<string, Property>();

            foreach(var property in properties)
            {
                dictionaryProperties.Add(property.Item1, new Property(property.Item2, property.Item3));
            }

            return dictionaryProperties;
        }

        public BaseStrategy(Guid networkID, List<Tuple<string , PropertyType, List<Tuple<string, object>>>> properties)
        {
            NetworkID = networkID;
            Properties = new Dictionary<string, Property>();

            foreach(var property in properties)
            {
                Properties.Add(property.Item1, new Property(property.Item2, property.Item3));
            }
        }

        public BaseStrategy(Guid networkID, Dictionary<string, Property> properties)
        {
            NetworkID = networkID;
            Properties = new Dictionary<string, Property>();

            foreach (var property in properties)
            {
                Properties.Add(property.Key, property.Value);
            }
        }

        public enum RoutingStrategies { Random, LinearRewardInaction, LinearRewardPenalty };
        public static List<string> RoutingStrategiesList = new List<string>{ "Random", "LinearRewardInaction", "LinearRewardPenalty" };
        public enum PickingStrategies { Random, FIFO, RandomRemoveUnreachable };
        public static List<string> PickingStrategiesList = new List<string> { "Random", "FIFO", "RandomRemoveUnreachable" };
        public enum CreationStrategies { Random };
        public static List<string> CreationStrategiesList = new List<string> { "Random" };

        public static RoutingStrategies GetRoutingStrategiesEnum(string strategy)
        {
            switch (strategy)
            {
                case "Random":
                    return RoutingStrategies.Random;
                case "LinearRewardInaction":
                    return RoutingStrategies.LinearRewardInaction;
                case "LinearRewardPenalty":
                    return RoutingStrategies.LinearRewardPenalty;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Dictionary<string, Property> GetRoutingStrategyProperties(string strategy)
        {
            switch (strategy)
            {
                case "Random":
                    return RandomRoutingStrategy.GetProperties();
                case "LinearRewardInaction":
                    return LinearRewardInactionRoutingStrategy.GetProperties();
                case "LinearRewardPenalty":
                    return LinearRewardPenaltyRoutingStrategy.GetProperties();
                default:
                    throw new NotImplementedException();
            }
        }

        public static PickingStrategies GetPickingStrategiesEnum(string strategy)
        {
            switch (strategy)
            {
                case "Random":
                    return PickingStrategies.Random;
                case "FIFO":
                    return PickingStrategies.FIFO;
                case "RandomRemoveUnreachable":
                    return PickingStrategies.RandomRemoveUnreachable;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Dictionary<string, Property> GetPickingStrategyProperties(string strategy)
        {
            switch (strategy)
            {
                case "Random":
                    return RandomPacketPickingStrategy.GetProperties();
                case "FIFO":
                    return FIFOPacketPickingStrategy.GetProperties();
                case "RandomRemoveUnreachable":
                    return RandomRemoveUnreachablePacketsStrategy.GetProperties();
                default:
                    throw new NotImplementedException();
            }
        }

        public static CreationStrategies GetCreationStrategiesEnum(string strategy)
        {
            switch (strategy)
            {
                case "Random":
                    return CreationStrategies.Random;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Dictionary<string, Property> GetCreationStrategyProperties(string strategy)
        {
            switch (strategy)
            {
                case "Random":
                    return RandomPacketCreationStrategy.GetProperties();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
