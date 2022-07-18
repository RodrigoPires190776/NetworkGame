using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector.NetworkDataComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkGameDataCollector
{
    public class NetworkData
    {
        public Dictionary<Guid, RouterData> RouterData { get; }
        //private Network.Network Network;
        public Dictionary<int, UpdatedState> States { get; }
        public List<decimal> AverageVariances { get; }
        private int NumberOfSteps;

        public NetworkData(Network.Network network)
        {
            //Network = network;
            RouterData = new Dictionary<Guid, RouterData>();
            States = new Dictionary<int, UpdatedState>();
            AverageVariances = new List<decimal>();
            NumberOfSteps = 0;

            foreach(var router in network.RouterIDList)
            {
                RouterData.Add(router, new RouterData(router));
            }       
        }

        public void AddEventHandler(Game game)
        {
            game.GameStep += Update;
        }

        private void Update(object sender, UpdatedState state)
        {
            foreach(var router in state.UpdatedRouters.Values)
            {
                RouterData[router.ID].Update(router);
            }

            foreach(var packet in state.UpdatedPackets.Values)
            {
                RouterData[packet.Source].Update(packet);
                RouterData[packet.Destination].Update(packet);
            }
            States.Add(state.NumberOfSteps, state);

            if (state.UpdatedAveragevariance) AverageVariances.Add(state.AverageVarience);
            NumberOfSteps = state.NumberOfSteps > NumberOfSteps ? state.NumberOfSteps : NumberOfSteps;
        }

        public void Update(UpdatedState state)
        {
            foreach (var router in state.UpdatedRouters.Values)
            {
                RouterData[router.ID].Update(router);
            }

            foreach (var packet in state.UpdatedPackets.Values)
            {
                RouterData[packet.Source].Update(packet);
                RouterData[packet.Destination].Update(packet);
            }
        }
    }
}
