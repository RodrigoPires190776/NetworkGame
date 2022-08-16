using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;

namespace Network.Strategies.PacketCreation
{
    public sealed class RandomPacketCreationStrategy : PacketCreationStrategy
    {
        public static Dictionary<string, Property> GetProperties()
        {
            var properties = new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.Probability, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 1m),
                    })
            };

            var dictionaryProperties = RoutingStrategy.GetProperties(properties);

            dictionaryProperties[Property.Probability].SetValue(0.1m);

            return dictionaryProperties;
        }
        public RandomPacketCreationStrategy(Guid networkID) : 
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
            Properties[Property.Probability].SetValue(0.1m);
        }

        public RandomPacketCreationStrategy(Guid networkID, Dictionary<string, Property> properties) :
            base(networkID, properties)

        { }

        public override Packet CreatePacket(Router router)
        {
            if ((decimal)new Random().NextDouble() < (decimal)Properties[Property.Probability].Value)
            {
                Guid dstID = router.ID;
                var network = NetworkMaster.GetInstance().GetNetwork(router.NetworkID);

                while (dstID == router.ID)
                {
                    var destination = new Random().Next(network.Routers.Count);
                    dstID = network.RouterIDList[destination];
                }

                return new Packet(router.ID, dstID, router.NetworkID);
            }
            return null;
        }
    }
}
