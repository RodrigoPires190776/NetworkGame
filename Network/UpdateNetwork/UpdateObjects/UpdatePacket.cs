using System;

namespace Network.UpdateNetwork.UpdateObjects
{
    public class UpdatePacket : UpdateObject
    {
        public int NumberOfSteps { get; }
        public bool ReachedDestination { get; }
        public UpdatePacket(Guid id, int nSteps, bool reached) : base(id)
        {
            NumberOfSteps = nSteps;
            ReachedDestination = reached;
        }
    }
}
