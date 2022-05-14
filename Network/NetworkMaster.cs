using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class NetworkMaster
    {
        private static object InstanceLock = new();
        private static NetworkMaster Instance;
        private Dictionary<Guid, Network> Networks;

        public static int PacketTTL = 50;

        private NetworkMaster()
        {
            Networks = new();
            Instance = this;
        }

        public static NetworkMaster GetInstance()
        {
            if(Instance == null)
            {
                lock (InstanceLock)
                {
                    if (Instance == null) Instance = new();
                }                
            }
            return Instance;
        }

        public void AddNetwork(Network network)
        {
            Networks.Add(network.ID, network);
        }

        public Network GetNetwork(Guid Id)
        {
            return Networks[Id];
        }
    }
}
