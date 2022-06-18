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
        public Dictionary<Guid, UpdateRouter> UpdatedRouters { get; }
        public Dictionary<Guid, UpdateLink> UpdatedLinks { get; }
        public Dictionary<Guid, UpdatePacket> UpdatedPackets { get; }
        public UpdatedState(Guid networkID, int numberOfSteps)
        {
            NetworkID = networkID;
            NumberOfSteps = numberOfSteps;
            UpdatedRouters = new Dictionary<Guid, UpdateRouter>();
            UpdatedLinks = new Dictionary<Guid, UpdateLink>();
            UpdatedPackets = new Dictionary<Guid, UpdatePacket>();
        }
    }
}
