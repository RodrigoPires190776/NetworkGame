using Network.Components;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Network.Strategies.PacketPicking
{
    public class RandomRemoveUnreachablePacketsStrategy : PacketPickingStrategy
    {
        public static Dictionary<string, Property> GetProperties()
        {
            return PacketPickingStrategy.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()); ;
        }
        public RandomRemoveUnreachablePacketsStrategy(Guid networkID) :
            base(networkID, new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            { })
        { }

        public RandomRemoveUnreachablePacketsStrategy(Guid networkID, Dictionary<string, Property> properties) :
            base(networkID, properties)
        { }

        public override (Packet, bool) NextPacket(Router router)
        {
            Guid packetID = Guid.Empty;
            var queueList = new List<Guid>();
            var queueDict = new Dictionary<Guid, Packet>();
            foreach(var p in router.PacketQueue)
            {
                queueList.Add(p.ID);
                queueDict.Add(p.ID, p);
            }

            while (queueList.Count > 0 && packetID == Guid.Empty)
            {
                packetID = queueList[new Random().Next(queueList.Count)];
                queueList.Remove(packetID);
                if (!CanReachDestination(router, queueDict[packetID]))
                {
                    router.PacketsToDrop.Add(queueDict[packetID]);
                    packetID = Guid.Empty;
                }
            }
            return packetID == Guid.Empty ? (null, true) : (queueDict[packetID], true);
        }

        public override void AddPacket(Packet packet)
        { }

        private bool CanReachDestination(Router router, Packet packet)
        {
            var distances = NetworkMaster.GetInstance().GetNetwork(router.NetworkID).RouterDistances;

            return distances[router.ID][packet.Destination] < NetworkMaster.PacketTTL - packet.NumberOfSteps;
        }
    }
}
