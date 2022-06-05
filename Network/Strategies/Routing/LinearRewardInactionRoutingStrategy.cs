using Network.Components;
using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Network.Strategies.Routing
{
    public class LinearRewardInactionRoutingStrategy : RoutingStrategy
    {
        public LinearRewardInactionRoutingStrategy(Guid routerID, Guid networkID) :
            base(routerID, networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>("Learning Weight", Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m)
                    })
            }) 
        { 
            Properties["Learning Weight"].SetValue(1m); 
        }

        public override void Learn(Packet packet)
        {
            if (!packet.ReachedDestination) return;
            var links = NetworkMaster.GetInstance().GetNetwork(NetworkID).Routers[RouterID].Links;

            RoutingTable.UpdateValue(packet.Destination, packet.RouterSentToLink[RouterID], ((decimal)Properties["Learning Weight"].Value)/packet.NumberOfSteps);
        }
    }
}
