using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;

namespace Network.UpdateNetwork
{
    public class UpdatedState : EventArgs
    {
        public Guid NetworkID { get; }
        public List<UpdateRouter> UpdatedRouters { get; }
        public List<UpdateLink> UpdatedLinks { get; }
        public List<UpdatePacket> UpdatedPackets { get; }
        public UpdatedState(Guid networkID)
        {
            NetworkID = networkID;
        }
    }
}
