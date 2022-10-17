using System;

namespace Network.UpdateNetwork.UpdateObjects
{
    public class UpdatePacket : UpdateObject
    {
        public int NumberOfSteps { get; }
        public bool ReachedDestination { get; }
        public bool Dropped { get; }
        public Guid Source { get; }
        public Guid Destination { get; }
        public UpdatePacket(Guid id, int nSteps, bool reached, bool dropped, Guid src, Guid dst) : base(id)
        {
            NumberOfSteps = nSteps;
            ReachedDestination = reached;
            Dropped = dropped;
            Source = src;
            Destination = dst;
        }
    }
}
