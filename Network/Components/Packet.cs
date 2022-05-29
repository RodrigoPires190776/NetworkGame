﻿using System;
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
        public Guid CurrentRouter { get; private set; }
        public bool ReachedDestination { get { return CurrentRouter == Destination; } }
        public int NumberOfSteps { get; private set; }

        public Packet(Guid src, Guid dst, Guid networkID)
        {
            NetworkID = networkID;
            ID = Guid.NewGuid();
            Source = src;
            Destination = dst;
            CurrentRouter = src;
            NumberOfSteps = 0;
        }

        public void Send(Guid curr)
        {
            CurrentRouter = curr;
        }

        public void Step()
        {
            NumberOfSteps++;
        }
    }
}
