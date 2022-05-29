using Network.Components;
using System;
using System.Collections.Generic;
using static Network.Components.Link;

namespace Network.UpdateNetwork.UpdateObjects
{
    public class UpdateLink : UpdateObject
    {
        public Dictionary<Guid, TransitInfo> PackagesInTransit { get; }
        public UpdateLink(Guid id, Dictionary<Packet, TransitInfo> packets) : base(id) 
        {
            PackagesInTransit = new Dictionary<Guid, TransitInfo>();
            foreach(var packet in packets.Keys)
            {
                PackagesInTransit.Add(packet.ID, packets[packet]);
            }
        }
    }
}
