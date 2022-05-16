using System;
using System.Collections.Generic;
using System.Linq;

namespace Network.Components
{
    public class Packet
    {
        public Guid NetworkID { get; }
        public Guid ID { get; }
        public Guid Source { get; }
        public Guid Destination { get; }
        public List<Guid> RouteTaken { get; }
        public Guid CurrentRouter { get { return RouteTaken.Last(); } }
        public bool ReachedDestination { get { return RouteTaken.Last() == Destination; } }
        public int NumberOfSteps { get; private set; }

        public Packet(Guid src, Guid dst, Guid networkID)
        {
            NetworkID = networkID;
            ID = new();
            Source = src;
            Destination = dst;
            RouteTaken = new();
            RouteTaken.Add(src);
            NumberOfSteps = 0;
        }

        public void Send(Guid curr)
        {
            RouteTaken.Add(curr);
        }

        public void Step()
        {
            NumberOfSteps++;
        }
    }
}
