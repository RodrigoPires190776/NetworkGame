using Network.UpdateNetwork;
using System;
using System.Collections.Generic;

namespace NetworkGameFrontend.NetworkApplication
{
    public class NetworkUpdateStateQueue
    {
        private Dictionary<Guid,UpdatedState> LatestState;
        private Dictionary<Guid, int> LastStateReturned;
        private object _queueLock = new object();

        public NetworkUpdateStateQueue()
        {
            LatestState = new Dictionary<Guid, UpdatedState>();
            LastStateReturned = new Dictionary<Guid, int>();
        }

        public void AddState(UpdatedState state)
        {
            lock (_queueLock)
            {
                if (!LatestState.ContainsKey(state.NetworkID)) 
                {
                    LatestState.Add(state.NetworkID, state);
                    LastStateReturned.Add(state.NetworkID, 0);
                    return;
                } 
                if(state.NumberOfSteps > LatestState[state.NetworkID].NumberOfSteps)
                {
                    LatestState[state.NetworkID] = state;
                }
            }           
        }

        public UpdatedState GetState(Guid networkID)
        {
            lock (_queueLock)
            {
                if (!LatestState.ContainsKey(networkID)) return null;
                if (LastStateReturned[networkID] >= LatestState[networkID].NumberOfSteps) return null;
                LastStateReturned[networkID] = LatestState[networkID].NumberOfSteps;
                return LatestState[networkID];
            }
        }
    }
}
