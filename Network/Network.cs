using Network.Components;
using Network.RouteDiscovery;
using Network.Strategies;
using Network.UpdateNetwork;
using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;

namespace Network
{
    public class Network
    {
        public string Name { get; set; }
        public int NetworkID { get; set; }
        public Dictionary<Guid, Router> Routers { get; private set; }
        public List<Guid> RouterIDList { get; private set; }
        public List<Link> Links { get; private set; }
        public Dictionary<Guid, Packet> Packets { get; private set; }
        public Guid ID { get; private set; }
        public int NumberOfSteps { get; private set; }
        private decimal AverageVariance { get; set; }
        private int StepsVariance { get; set; }
        public Guid AttackerID { get; private set; }
        public Guid DefensorID { get; private set; }
        public Guid DestinationID { get; private set; }
        public Dictionary<Guid,Dictionary<Guid,int>> RouterDistances { get; private set; }

        public Network()
        {
            Routers = new Dictionary<Guid, Router>();
            RouterIDList = new List<Guid>();
            Links = new List<Link>();
            Packets = new Dictionary<Guid, Packet>();
            ID = Guid.NewGuid();
            NetworkID = 0;
            NumberOfSteps = 0;
            AverageVariance = 0;
            StepsVariance = 0;
            AttackerID = Guid.Empty;
            DefensorID = Guid.Empty;
            DestinationID = Guid.Empty;
        }

        public Network Copy() 
        {
            var network = new Network();
            network.Name = Name;
            
            NetworkMaster.GetInstance().AddNetwork(network, network.Name);
            var routerIDmapping = new Dictionary<Guid, Guid>();
          
            foreach(var router in Routers.Values)
            {
                var newRouter = new Router(network.ID, router.Coordinates);
                network.AddRouter(newRouter);
                routerIDmapping.Add(router.ID, newRouter.ID);
            }
            foreach(var link in Links)
            {
                network.AddLink(new Link(routerIDmapping[link.Routers.Item1], routerIDmapping[link.Routers.Item2], link.LinkLength, network.ID));
            }

            network.RouterDistances = new Dictionary<Guid, Dictionary<Guid, int>>();
            foreach(var source in RouterDistances.Keys)
            {
                var dict = new Dictionary<Guid, int>();
                foreach(var destination in RouterDistances[source].Keys)
                {
                    dict.Add(routerIDmapping[destination], RouterDistances[source][destination]);
                }
                network.RouterDistances.Add(routerIDmapping[source], dict);
            }

            return network;
        }

        public void AddRouter(Router router)
        {
            Routers.Add(router.ID, router);
            RouterIDList.Add(router.ID);
        }

        public void AddLink(Link link)
        {
            Links.Add(link);
            Routers[link.Routers.Item1].AddLink(link);
            Routers[link.Routers.Item2].AddLink(link);
        }

        public void SetStrategies(Guid routerID, RoutingStrategy routing, PacketCreationStrategy packetCreation, PacketPickingStrategy packetPicking)
        {
            Routers[routerID].SetStrategies(routing, packetCreation, packetPicking);
        }

        public void IntroduceAttacker(Guid defensorID, Guid destinationID, Guid attackerID)
        {
            Routers[defensorID].SetAgentDefensor(destinationID);
            Routers[attackerID].SetAgentAttacker(defensorID);
            DefensorID = defensorID;
            DestinationID = destinationID;
            AttackerID = attackerID;
        }

        public void IntroduceAttackerRandom()
        {
            int destination = new Random().Next(RouterIDList.Count);
            int defensor = destination;
            while(defensor == destination)
            {
                defensor = new Random().Next(RouterIDList.Count);
            }

            var destinationID = RouterIDList[destination];
            var defensorID = RouterIDList[defensor];

            var routerList = new List<Guid>();
            bool foundDestination = false;

            foreach(var neighbourLink in Routers[defensorID].Links.Values)
            {
                var neighbour = GetOtherRouter(defensorID, neighbourLink);
                routerList.Add(neighbour);
                if (neighbour == destinationID) foundDestination = true;
            }

            if (!foundDestination) routerList.Clear();
            Guid currentRouter = defensorID;
            while (!foundDestination)
            {
                var router = currentRouter;
                foreach (var neighbourLink in Routers[currentRouter].Links.Values)
                {
                    var neighbour = GetOtherRouter(currentRouter, neighbourLink);
                    if (neighbour == destinationID) foundDestination = true;
                    if (RouterDistances[neighbour][destinationID] < RouterDistances[router][destinationID])
                    {
                        router = neighbour;
                    }
                }
                routerList.Add(router);
                currentRouter = router;
            }
            routerList.Remove(destinationID);

            int attacker = new Random().Next(routerList.Count);
            var attackerID = routerList[attacker];

            this.IntroduceAttacker(defensorID, destinationID, attackerID);
        }

        public (Guid, Guid, Guid) GetNetworkAgents()
        {
            return (DefensorID, DestinationID, AttackerID);
        }

        public void Initialize(BaseRouteDiscovery discovery)
        {
            discovery.Discovery();
        }

        public UpdatedState Step()
        {
            var state = new UpdatedState(ID, ++NumberOfSteps);

            foreach(var packet in Packets.Values)
            {
                packet.Step();
            }

            foreach (Link link in Links)
            {
                var result = link.Step();

                state.UpdatedLinks.Add(link.ID, result.Item2);

                //Expired
                foreach(var packet in result.Item3)
                {
                    state.AddUpdatePacket(new UpdatePacket(packet.ID, packet.NumberOfSteps, false, true, packet.Source, packet.Destination));
                    Packets.Remove(packet.ID);

                    foreach(var router in packet.RouterSentToLink.Keys)
                    {
                        Routers[router].Learn(packet);                       
                    }
                }

                //Reached Router
                foreach (Packet packet in result.Item1)
                {
                    state.AddUpdatePacket(new UpdatePacket(packet.ID, packet.NumberOfSteps, packet.ReachedDestination, false, packet.Source, packet.Destination));

                    if (!packet.ReachedDestination)
                    {
                        Routers[packet.CurrentRouter].AddPacket(packet);
                    }
                    else
                    {
                        foreach (var router in packet.RouterSentToLink.Keys)
                        {
                            Routers[router].Learn(packet);
                        }
                    }
                }                 
            }

            
            foreach(Router router in Routers.Values)
            {
                var updateRouter = router.Step(); //UpdateRouter, NewPacket, DroppedPacket
                if (updateRouter.Item2 != null) Packets.Add(updateRouter.Item2.ID, updateRouter.Item2);
                state.UpdatedRouters.Add(router.ID, updateRouter.Item1);
                if (updateRouter.Item3 != null)
                {
                    state.AddUpdatePacket(new UpdatePacket(updateRouter.Item3.ID,
                    updateRouter.Item3.NumberOfSteps, false, true, updateRouter.Item3.Source, updateRouter.Item3.Destination));
                    foreach (var routerID in updateRouter.Item3.RouterSentToLink.Keys)
                    {
                        Routers[routerID].Learn(updateRouter.Item3);
                    }
                }

            }

            StepsVariance++;
            if(StepsVariance >= NetworkMaster.AverageVarianceUpdateRate)
            {
                state.UpdatedAveragevariance = true;
                decimal averageVarience = 0m;
                foreach (var router in RouterIDList)
                {
                    averageVarience += Routers[router].GetVariance();
                }
                AverageVariance = averageVarience / Routers.Count; ;
               
                state.AverageVarience = AverageVariance / StepsVariance;
                StepsVariance = 0;
            }
            
            
            return state;
        }

        public NetworkInfo GetNetworkInfo()
        {
            int maxLenght = 0;
            int numberOfPaths = 0;
            int lenghtSum = 0;

            foreach(var router in RouterDistances.Keys)
            {
                foreach(var destination in RouterDistances[router].Keys)
                {
                    if(router != destination)
                    {
                        if (RouterDistances[router][destination] > maxLenght) maxLenght = RouterDistances[router][destination];
                        numberOfPaths++;
                        lenghtSum += RouterDistances[router][destination];
                    }                   
                }
            }

            return new NetworkInfo()
            {
                LongestPathLenght = maxLenght,
                AveragePathLenght = (decimal)lenghtSum / numberOfPaths
            };
        }

        public void ComputeRouterDistances()
        {
            RouterDistances = new Dictionary<Guid, Dictionary<Guid, int>>();
            foreach (var node in RouterIDList)
            {
                var nodeDistances = Dijkstra(node);
                RouterDistances.Add(node, nodeDistances);
            }
        }

        private Dictionary<Guid, int> Dijkstra(Guid node)
        {
            var nodeDistances = new Dictionary<Guid, int>();
            Guid nodeMin;
            int nodeMinValue;
            nodeDistances.Add(node, 0);
            var list = new List<Guid>() { node };

            while (list.Count > 0)
            {
                nodeMin = list[0];
                nodeMinValue = nodeDistances.ContainsKey(list[0]) ? nodeDistances[list[0]] : int.MaxValue;
                foreach (var n in list)
                {
                    if (nodeDistances.ContainsKey(n) && nodeDistances[n] < nodeMinValue)
                    {
                        nodeMin = n;
                        nodeMinValue = nodeDistances[n];
                    }
                }
                node = nodeMin;
                list.Remove(node);

                var router = Routers[node];

                foreach (var edge in router.Links.Values)
                {
                    var neighbour = GetOtherRouter(node, edge);
                    var cost = nodeDistances[node] + edge.LinkLength;
                    if (nodeDistances[node] == 0) cost -= 1;

                    if (!nodeDistances.ContainsKey(neighbour))
                    {
                        nodeDistances.Add(neighbour, cost);
                        list.Add(neighbour);
                    }
                    else if (cost < nodeDistances[neighbour])
                    {
                        nodeDistances[neighbour] = cost;
                    }
                }
            }

            return nodeDistances;
        }

        private Guid GetOtherRouter(Guid node, Link link)
        {
            return link.Routers.Item1 == node ? link.Routers.Item2 : link.Routers.Item1;
        }
    }

    public class NetworkInfo
    {
        public int LongestPathLenght { get; set; }
        public decimal AveragePathLenght { get; set; }
    }
}
