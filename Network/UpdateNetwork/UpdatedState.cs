using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;

namespace Network.UpdateNetwork
{
    public class UpdatedState : EventArgs
    {
        public Guid NetworkID { get; }
        public int NumberOfSteps { get; }
        public decimal AverageVarience { get; set; }
        public bool UpdatedAveragevariance { get; set; }
        public Dictionary<Guid, UpdateRouter> UpdatedRouters { get; }
        public Dictionary<Guid, UpdateLink> UpdatedLinks { get; }
        private Dictionary<Guid, UpdatePacket> UpdatedPackets { get; }
        public List<Guid> FinishedPackets { get; }
        public UpdatedState(Guid networkID, int numberOfSteps)
        {
            NetworkID = networkID;
            NumberOfSteps = numberOfSteps;
            UpdatedAveragevariance = false;
            UpdatedRouters = new Dictionary<Guid, UpdateRouter>();
            UpdatedLinks = new Dictionary<Guid, UpdateLink>();
            UpdatedPackets = new Dictionary<Guid, UpdatePacket>();
            FinishedPackets = new List<Guid>();
        }

        public Dictionary<Guid, UpdatePacket> GetUpdatePackets()
        {
            return UpdatedPackets;
        }

        public void AddUpdatePacket(UpdatePacket packet)
        {
            UpdatedPackets[packet.ID] = packet;
            if (packet.ReachedDestination || packet.Dropped) FinishedPackets.Add(packet.ID);
        }
    }
}
