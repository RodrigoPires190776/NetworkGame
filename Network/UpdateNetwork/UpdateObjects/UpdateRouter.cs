using Network.Strategies;
using System;

namespace Network.UpdateNetwork.UpdateObjects
{
    public class UpdateRouter : UpdateObject
    {
        public int NumberPacketsInQueue { get; }
        public bool PacketCreated { get; }
        public bool PacketSent { get; }
        public RoutingTable RoutingTable { get; }
        public UpdateRouter(Guid id, int nPackets, bool created, bool sent, RoutingTable table) : base(id)
        {
            NumberPacketsInQueue = nPackets;
            PacketCreated = created;
            PacketSent = sent;
            RoutingTable = table;
        }
    }
}
