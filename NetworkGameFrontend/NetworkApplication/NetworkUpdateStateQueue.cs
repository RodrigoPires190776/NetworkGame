using Network.UpdateNetwork;

namespace NetworkGameFrontend.NetworkApplication
{
    public class NetworkUpdateStateQueue
    {
        private UpdatedState LatestState;
        private object _queueLock = new object();

        public NetworkUpdateStateQueue()
        {

        }

        public void AddState(UpdatedState state)
        {
            lock (_queueLock)
            {
                if (LatestState == null || state.NumberOfSteps > LatestState.NumberOfSteps)
                {
                    LatestState = state;
                }
            }           
        }

        public UpdatedState GetState()
        {
            lock (_queueLock)
            {
                return LatestState;
            }
        }
    }
}
