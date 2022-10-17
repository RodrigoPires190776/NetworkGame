using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Network.Strategies.DefenderStrategies
{
    public class OneDestinationPacketCreationStrategy : PacketCreationStrategy
    {
        private readonly Guid DestinationID;
        public OneDestinationPacketCreationStrategy(Guid networkID, Guid destinationID, decimal probability) :
            base(networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.Probability, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 1m),
                    })
            })
        {
            Properties[Property.Probability].SetValue(probability);
            DestinationID = destinationID;
        }

        public override Packet CreatePacket(Router router)
        {
            if ((decimal) new Random().NextDouble() < (decimal)Properties[Property.Probability].Value)
            {
                return new Packet(router.ID, DestinationID, router.NetworkID);
            }
            return null;
        }
    }
}
