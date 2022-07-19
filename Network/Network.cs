using Network.Components;
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
        public Dictionary<Guid, Router> Routers { get; private set; }
        public List<Guid> RouterIDList { get; private set; }
        public List<Link> Links { get; private set; }
        public Dictionary<Guid, Packet> Packets { get; private set; }
        public Guid ID { get; private set; }
        public int NumberOfSteps { get; private set; }
        private decimal AverageVariance { get; set; }
        private int StepsVariance { get; set; }

        public Network()
        {
            Routers = new Dictionary<Guid, Router>();
            RouterIDList = new List<Guid>();
            Links = new List<Link>();
            Packets = new Dictionary<Guid, Packet>();
            ID = Guid.NewGuid();
            NumberOfSteps = 0;
            AverageVariance = 0;
            StepsVariance = 0;
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
                    state.UpdatedPackets.Add(packet.ID, new UpdatePacket(packet.ID, packet.NumberOfSteps, false, true, packet.Source, packet.Destination));
                    Packets.Remove(packet.ID);

                    foreach(var router in packet.RouterSentToLink.Keys)
                    {
                        Routers[router].Learn(packet);                       
                    }
                }

                //Reached Router
                foreach (Packet packet in result.Item1)
                {
                    state.UpdatedPackets.Add(packet.ID, new UpdatePacket(packet.ID, packet.NumberOfSteps, packet.ReachedDestination, false, packet.Source, packet.Destination));

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
                    state.UpdatedPackets[updateRouter.Item3.ID] = new UpdatePacket(updateRouter.Item3.ID,
                    updateRouter.Item3.NumberOfSteps, false, true, updateRouter.Item3.Source, updateRouter.Item3.Destination);
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
    }
}
