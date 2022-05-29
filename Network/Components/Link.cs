using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;

namespace Network.Components
{
    public class  Link
    {
        public Guid NetworkID { get; }
        public Guid ID { get; }
        public Tuple<Guid, Guid> Routers { get; }
        public Dictionary<Packet, TransitInfo> PackagesInTransit { get; }
        public int LinkLength { get; }

        public Link(Guid r1, Guid r2, int length, Guid networkID)
        {
            Routers = new Tuple<Guid, Guid>(r1, r2);
            PackagesInTransit = new Dictionary<Packet, TransitInfo>();
            LinkLength = length;
            NetworkID = networkID;
            ID = Guid.NewGuid();
        }

        public (List<Packet>, UpdateLink, List<UpdatePacket>) Step()
        {
            List<Packet> reachedRouter = new List<Packet>();
            List<Packet> expired = new List<Packet>();

            foreach (Packet packet in PackagesInTransit.Keys)
            {
                packet.Step();
                PackagesInTransit[packet].PositionInLink += PackagesInTransit[packet].Direction;
                if (ReachedEnd(PackagesInTransit[packet]))
                {
                    reachedRouter.Add(packet);
                    packet.Send(packet.CurrentRouter == Routers.Item1 ? Routers.Item2 : Routers.Item1);
                }
                else if (packet.NumberOfSteps >= NetworkMaster.PacketTTL) expired.Add(packet);
            }

            foreach (Packet packet in reachedRouter)
            {
                PackagesInTransit.Remove(packet);
            }

            var expiredList = new List<UpdatePacket>();
            foreach (Packet packet in expired)
            {
                PackagesInTransit.Remove(packet);
                expiredList.Add(new UpdatePacket(packet.ID, packet.NumberOfSteps, false, true, packet.Source, packet.Destination));
            }

            var state = new UpdateLink(ID, PackagesInTransit, reachedRouter, expired);

            return (reachedRouter, state, expiredList);
        }

        public void Send(Router router, Packet packet)
        {
            TransitInfo info = router.ID == Routers.Item1 ? 
                new TransitInfo(0, 1, packet.NumberOfSteps) : new TransitInfo(LinkLength - 1, -1, packet.NumberOfSteps);
            PackagesInTransit.Add(packet, info);
        }

        private bool ReachedEnd(TransitInfo info)
        {
            return info.Direction > 0 ? info.PositionInLink >= LinkLength - 1 : info.PositionInLink <= 0;
        }

        public class TransitInfo
        {
            public int PositionInLink { get; set;  }
            public int Direction { get; }
            public int NrSteps { get; }
            public TransitInfo (int position, int direction, int nrSteps)
            {
                PositionInLink = position;
                Direction = direction;
                NrSteps = nrSteps;
            }
        }
    }

    
}
