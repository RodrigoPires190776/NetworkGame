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
        private readonly Dictionary<string, List<string>> NetworkNames;

        public static int PacketTTL = 100;
        public static int AverageVarianceUpdateRate = 100;

        private NetworkMaster()
        {
            Networks = new Dictionary<Guid, Network>();
            NetworksByName = new Dictionary<string, Network>();
            NetworkNames = new Dictionary<string, List<string>>();
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
            network.NetworkID = AddNetworkName(network, networkName);
            network.Name = networkName;
        }

        private int AddNetworkName(Network network, string networkName)
        {
            if (!NetworksByName.ContainsKey(networkName))
            {
                NetworksByName.Add(networkName, network);
                NetworkNames.Add(networkName, new List<string>());
                return 0;
            }
            if (!NetworksByName.ContainsKey(networkName + "_1"))
            {
                NetworksByName.Add(networkName + "_1", network);
                NetworkNames[networkName].Add(networkName + "_1");
                return 1;
            }
            var newName = networkName + $"_{NetworkNames[networkName].Count + 1}";
            NetworksByName.Add(newName, network);
            NetworkNames[networkName].Add(newName);
            return NetworkNames[networkName].Count;
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
