using System;
using System.Collections.Generic;

namespace Network.Components
{
    public class  Link
    {
        public Guid NetworkID { get; }
        public Tuple<int, int> Routers { get; }
        public Dictionary<Packet, TransitInfo> PackagesInTransit { get; }
        private int LinkLength { get; }

        public Link(int r1, int r2, int length, Guid networkID)
        {
            Routers = new(r1, r2);
            PackagesInTransit = new();
            LinkLength = length;
            NetworkID = networkID;
        }

        public List<Packet> Step()
        {
            List<Packet> reachedRouter = new();
            List<Packet> expired = new();

            foreach (Packet packet in PackagesInTransit.Keys)
            {
                packet.Step();
                if (ReachedEnd(PackagesInTransit[packet])) reachedRouter.Add(packet);
                else if (packet.NumberOfSteps >= NetworkMaster.PacketTTL) expired.Add(packet);
            }

            foreach (Packet packet in reachedRouter)
            {
                PackagesInTransit.Remove(packet);
            }

            foreach (Packet packet in expired)
            {
                PackagesInTransit.Remove(packet);
            }

            return reachedRouter;
        }

        public void Send(Router router, Packet packet)
        {
            TransitInfo info = router.ID == Routers.Item1 ? new(0, 1) : new(LinkLength, -1);
            PackagesInTransit.Add(packet, info);
        }

        private bool ReachedEnd(TransitInfo info)
        {
            return info.Direction > 0 ? info.PositionInLink >= LinkLength : info.PositionInLink <= 0;
        }

        public class TransitInfo
        {
            public int PositionInLink { get; }
            public int Direction { get; }
            public TransitInfo (int position, int direction)
            {
                PositionInLink = position;
                Direction = direction;
            }
        }
    }

    
}
