using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Network.Strategies.PacketPicking
{
    public class OneSourceRemovePacketPickingStrategy : PacketPickingStrategy
    {
        private readonly Guid SourceID;
        public OneSourceRemovePacketPickingStrategy(Guid networkID, Guid sourceID) :
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
            Properties["Probability"].SetValue(100m);
            SourceID = sourceID;
        }
        public override (Packet, bool) NextPacket(Router router)
        {
            Packet packet;
            if(router.PacketQueue.Count > 0)
            {
                packet = router.PacketQueue[new Random().Next(router.PacketQueue.Count)];
                if (packet.Source != SourceID) return (packet, true);
                else 
                {
                    if (new Random().Next(100000) < (decimal)Properties["Probability"].Value * 1000)
                    {
                        return (packet, false);
                    }
                    else return (packet, true);
                }

            }
            return (null, true);
        }
    }
}
