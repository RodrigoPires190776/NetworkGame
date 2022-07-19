using Network.Components;
using Network.UpdateNetwork.UpdateObjects;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Network.Strategies.Routing
{
    public class LinearRewardPenaltyRoutingStrategy : RoutingStrategy
    {
        public static Dictionary<string, Property> GetProperties()
        {
            var properties = new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.LearningWeight, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m)
                    })
            };

            var dictionaryProperties = RoutingStrategy.GetProperties(properties);

            dictionaryProperties[Property.LearningWeight].SetValue(1m);

            return dictionaryProperties;
        }

        public LinearRewardPenaltyRoutingStrategy(Guid routerID, Guid networkID) :
            base(routerID, networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.LearningWeight, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m)
                    })
            })
        {
            Properties[Property.LearningWeight].SetValue(1m);
        }

        public LinearRewardPenaltyRoutingStrategy(Guid routerID, Guid networkID, Dictionary<string, Property> properties) :
            base(routerID, networkID, properties)
        { }

        public override void Learn(Packet packet)
        {
            if (!packet.ReachedDestination) 
            {
                RoutingTable.UpdateValue(packet.Destination, packet.RouterSentToLink[RouterID],
                -(decimal)Properties[Property.LearningWeight].Value / (NetworkMaster.PacketTTL * NetworkMaster.PacketTTL));
            }
            else
            {
                RoutingTable.UpdateValue(packet.Destination, packet.RouterSentToLink[RouterID],
                ((decimal)Properties[Property.LearningWeight].Value) / (packet.NumberOfSteps * packet.NumberOfSteps));
            }          
        }
    }
}
