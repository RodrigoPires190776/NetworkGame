using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector.NetworkDataComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkGameDataCollector
{
    public class NetworkDataCollector
    {
        private static readonly object InstanceLock = new object();
        private static NetworkDataCollector Instance;
        private readonly Dictionary<Guid, NetworkData> Networks;

        private NetworkDataCollector()
        {
            Networks = new Dictionary<Guid, NetworkData>();
            Instance = this;
        }

        public static NetworkDataCollector GetInstance()
        {
            if (Instance == null)
            {
                lock (InstanceLock)
                {
                    if (Instance == null) Instance = new NetworkDataCollector();
                }
            }
            return Instance;
        }
        public void AddNetwork(Network.Network network)
        {
            var networkData = new NetworkData(network);
            Networks.Add(network.ID, networkData);
        }

        public void AddEventHandler(Guid networkID, Game game)
        {
            Networks[networkID].AddEventHandler(game);
        }

        public RouterData GetRouterData(Guid networkID, Guid routerID)
        {
            return Networks[networkID].RouterData[routerID].Copy();
        }

        public List<RouterData> GetPreviousRouterData(Guid networkID, Guid routerID)
        {
            var list = new List<RouterData>();

            var tempNetwork = new NetworkData(NetworkMaster.GetInstance().GetNetwork(networkID));

            foreach(var state in Networks[networkID].States.Values)
            {
                tempNetwork.Update(state);
                list.Add(tempNetwork.RouterData[routerID].Copy());
            }

            return list;
        }

        public List<decimal> GetPreviousAverageVariance(Guid networkID)
        {
            return Networks[networkID].AverageVariances;
        }
    }
}
