using Network.Strategies;
using Network.Strategies.AttackerStrategies;
using Network.Strategies.DefenderStrategies;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;

namespace Network.Components
{
    public class Router
    {
        public Coordinates Coordinates { get; private set; }
        public Guid ID { get; }
        public Guid NetworkID { get; }
        public Dictionary<Guid, Link> Links { get; }
        public List<Packet> PacketQueue { get;  }
        public RoutingStrategy RoutingStrategy { get; private set; }
        public PacketPickingStrategy PacketPickingStrategy { get; private set; }
        public PacketCreationStrategy PacketCreationStrategy { get; private set; }
        public RouterAgent RouterAgent { get; private set; }

        public Router(Guid networkID, Coordinates coordinates)
        {
            ID = Guid.NewGuid();
            Coordinates = coordinates;
            NetworkID = networkID;
            Links = new Dictionary<Guid, Link>();
            PacketQueue = new List<Packet>();
            RoutingStrategy = new RandomRoutingStrategy(ID, networkID);
            PacketPickingStrategy = new RandomPacketPickingStrategy(networkID);
            PacketCreationStrategy = new RandomPacketCreationStrategy(networkID);
            RouterAgent = RouterAgent.Normal;
        }

        protected Router() { }

        public void AddLink(Link link)
        {
            Links.Add(link.ID, link);
        }

        public void AddPacket(Packet packet)
        {
            PacketQueue.Add(packet);
        }

        public void SetStrategies(RoutingStrategy routing, PacketCreationStrategy packetCreation, PacketPickingStrategy packetPicking)
        {
            RoutingStrategy = routing ?? RoutingStrategy;
            RoutingStrategy.Initialize(NetworkMaster.GetInstance().GetNetwork(NetworkID), this);
            PacketCreationStrategy = packetCreation ?? PacketCreationStrategy;
            PacketPickingStrategy = packetPicking ?? PacketPickingStrategy;
        }

        public (UpdateRouter, Packet, List<Packet>) Step()
        {
            var dropped = new List<Packet>();
            foreach (var packet in PacketQueue)
            {
                if(packet.NumberOfSteps >= NetworkMaster.PacketTTL)
                {
                    dropped.Add(packet);
                }
            }

            foreach(var packet in dropped)
            {
                PacketQueue.Remove(packet);
            }

            var newPacket = PacketCreationStrategy.CreatePacket(this);
            if (newPacket != null) PacketQueue.Add(newPacket);

            (Packet, bool) nextPacket = (null, false);
            if (PacketQueue.Count > 0)
            {
                nextPacket = PacketPickingStrategy.NextPacket(this);
                if(nextPacket.Item2)
                {
                    RoutingStrategy.NextLink(this, nextPacket.Item1).Send(this, nextPacket.Item1);
                    PacketQueue.Remove(nextPacket.Item1);
                }
                else
                {
                    PacketQueue.Remove(nextPacket.Item1);
                    dropped.Add(nextPacket.Item1);
                }
            }
           
            return (new UpdateRouter(ID, PacketQueue.Count, newPacket != null, nextPacket.Item2, RoutingStrategy.RoutingTable), newPacket, dropped);
        }

        public void Learn(Packet packet)
        {
            RoutingStrategy.Learn(packet);
        }

        public decimal GetVariance()
        {
            return RoutingStrategy.RoutingTable.GetVariance();
        }

        public void SetAgentNormal()
        {
            RouterAgent = RouterAgent.Normal;
        }

        public void SetAgentDefensor(Guid destinationID)
        {
            PacketCreationStrategy = new OneDestinationPacketCreationStrategy(NetworkID, destinationID);
            PacketCreationStrategy.Properties["Probability"].SetValue(20m);
            RouterAgent = RouterAgent.Defensor;
        }

        public void SetAgentAttacker(Guid defensorID)
        {
            PacketPickingStrategy = new OneSourceRemovePacketPickingStrategy(NetworkID, defensorID);
            PacketPickingStrategy.Properties["Probability"].SetValue(100m);
            RouterAgent = RouterAgent.Attacker;
        }
    }

    public class Coordinates
    {
        public double X { get; }
        public double Y { get; }
        public Coordinates(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public enum RouterAgent { Normal, Defensor, Attacker }
}
