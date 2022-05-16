using System;

namespace Network.UpdateNetwork.UpdateObjects
{
    public class UpdateRouter : UpdateObject
    {
        public int NumberPacketsInQueue { get; }
        public UpdateRouter(Guid id, int nPackets) : base(id)
        {
            NumberPacketsInQueue = nPackets;
        }
    }
}
