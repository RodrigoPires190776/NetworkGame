using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkGameDataCollector.NetworkDataComponents
{
    public class LinkData
    {
        public Guid LinkID { get; }
        public int PacketsDelivered { get; }

        public LinkData(Guid id)
        {
            LinkID = id;
        }
    }
}
