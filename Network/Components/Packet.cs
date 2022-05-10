using System.Collections.Generic;
using System.Linq;

namespace Network.Components
{
    public class Packet
    {
        public static int TTL = 50;
        public int Source { get; }
        public int Destination { get; }
        public List<int> RouteTaken { get; }
        public int CurrentRouter { get { return RouteTaken.Last(); } }
        public bool ReachedDestination { get { return RouteTaken.Last() == Destination; } }
        public int NumberOfSteps { get; private set; }

        public Packet(int src, int dst)
        {
            Source = src;
            Destination = dst;
            RouteTaken = new();
            RouteTaken.Add(src);
            NumberOfSteps = 0;
        }

        public void Send(int curr)
        {
            RouteTaken.Add(curr);
        }

        public void Step()
        {
            NumberOfSteps++;
        }
    }
}
