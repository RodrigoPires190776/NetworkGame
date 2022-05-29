using System;

namespace Network.UpdateNetwork.UpdateObjects
{
    public class UpdateRouter : UpdateObject
    {
        public int NumberPacketsInQueue { get; }
        public bool PacketCreated { get; }
        public bool PacketSent { get; }
        public UpdateRouter(Guid id, int nPackets, bool created, bool sent) : base(id)
        {
            NumberPacketsInQueue = nPackets;
            PacketCreated = created;
            PacketSent = sent;
        }
    }
}
