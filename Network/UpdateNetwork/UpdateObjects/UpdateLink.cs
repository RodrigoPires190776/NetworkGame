using Network.Components;
using System;
using System.Collections.Generic;
using static Network.Components.Link;

namespace Network.UpdateNetwork.UpdateObjects
{
    public class UpdateLink : UpdateObject
    {
        public Dictionary<Guid, TransitInfo> PackagesInTransit { get; }
        public List<Guid> ReachedRouter { get; }
        public List<Guid> Expired { get; }
        public UpdateLink(Guid id, Dictionary<Packet, TransitInfo> packets, List<Packet> reachedRouter, List<Packet> expired) : base(id) 
        {
            PackagesInTransit = new Dictionary<Guid, TransitInfo>();
            foreach(var packet in packets.Keys)
            {
                PackagesInTransit.Add(packet.ID, packets[packet]);
            }

            ReachedRouter = new List<Guid>();
            foreach(var packet in reachedRouter)
            {
                ReachedRouter.Add(packet.ID);
            }

            Expired = new List<Guid>();
            foreach (var packet in expired)
            {
                Expired.Add(packet.ID);
            }
        }
    }
}
