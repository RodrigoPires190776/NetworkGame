using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkGameDataCollector.NetworkDataComponents
{
    public class RouterData
    {
        public Guid RouterID { get; private set; }
        public int PacketsCreated { get; private set; }
        public int PacketsSent { get; private set; }
        public int PacketsDelivered { get; private set; }
        public int PacketsDropped { get; private set; }
        public int PacketsInTransit { get; private set; }
        public int PacketsInQueue { get; private set; }
        public int PacketsReceived { get; private set; }
        public Dictionary<Guid, int> PacketDeliverTimes { get; }
        public decimal PacketAverageDeliverTime { get; private set; }
    
        public RouterData(Guid id)
        {
            RouterID = id;
            PacketsCreated = 0;
            PacketsSent = 0;
            PacketsDelivered = 0;
            PacketsDropped = 0;
            PacketsInTransit = 0;
            PacketsInQueue = 0;
            PacketsReceived = 0;
            PacketDeliverTimes = new Dictionary<Guid, int>();
            PacketAverageDeliverTime = 0;
        }

        public void Update(UpdateRouter data)
        {
            PacketsInQueue = data.NumberPacketsInQueue;
            if (data.PacketCreated)
            {
                PacketsCreated++;
                PacketsInTransit++;
            }           
            if (data.PacketSent) { PacketsSent++;  }
        }

        public void Update(UpdatePacket data)
        {
            if(data.Destination == RouterID)
            {
                if(data.ReachedDestination) PacketsReceived++; 
                return;
            }
            
            if (data.ReachedDestination)
            {
                PacketsInTransit--;
                PacketAverageDeliverTime = (PacketAverageDeliverTime * PacketsDelivered + data.NumberOfSteps) / (PacketsDelivered + 1);
                PacketsDelivered++;
                PacketDeliverTimes.Add(data.ID, data.NumberOfSteps);
            }
            if (data.Dropped)
            {
                PacketsDropped++;
                PacketsInTransit--;
            }
        }
    }
}
