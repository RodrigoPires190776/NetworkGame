using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkGameDataCollector.NetworkDataComponents
{
    public class PacketData
    {
        public Guid PacketID { get; }
        public List<Guid> RouteTaken { get; }
        public Dictionary<Guid, int> TimeInRouter { get; }
        public int NumberOfSteps { get; }

        public PacketData(Guid id)
        {
            PacketID = id;
            RouteTaken = new List<Guid>();
            TimeInRouter = new Dictionary<Guid, int>();
        }
    }
}
