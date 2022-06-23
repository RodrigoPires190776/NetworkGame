using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Network.Strategies.PacketCreation
{
    public class OneDestinationPacketCreationStrategy : PacketCreationStrategy
    {
        private readonly Guid DestinationID;
        public OneDestinationPacketCreationStrategy(Guid networkID, Guid destinationID) :
            base(networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>("Probability", Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 100m),
                    })
            })
        {
            Properties["Probability"].SetValue(10m);
            DestinationID = destinationID;
        }

        public override Packet CreatePacket(Router router)
        {
            if (new Random().Next(100000) < (decimal)Properties["Probability"].Value * 1000)
            {
                return new Packet(router.ID, DestinationID, router.NetworkID);
            }
            return null;
        }
    }
}
