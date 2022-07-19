using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class NetworkMaster
    {
        private static readonly object InstanceLock = new object();
        private static NetworkMaster Instance;
        private readonly Dictionary<Guid, Network> Networks;
        private readonly Dictionary<string, Network> NetworksByName;

        public static int PacketTTL = 100;
        public static int AverageVarianceUpdateRate = 100;

        private NetworkMaster()
        {
            Networks = new Dictionary<Guid, Network>();
            NetworksByName = new Dictionary<string, Network>();
            Instance = this;
        }

        public static NetworkMaster GetInstance()
        {
            if(Instance == null)
            {
                lock (InstanceLock)
                {
                    if (Instance == null) Instance = new NetworkMaster();
                }                
            }
            return Instance;
        }

        public void AddNetwork(Network network, string networkName)
        {
            Networks.Add(network.ID, network);
            NetworksByName.Add(networkName, network);
            network.Name = networkName;
        }

        public Network GetNetwork(Guid Id)
        {
            return Networks[Id];
        }

        public Network GetNetworkByName(string name)
        {
            return NetworksByName[name];
        }

        public List<string> GetAllNetworkNames()
        {
            return NetworksByName.Keys.ToList<string>();
        }
    }
}
