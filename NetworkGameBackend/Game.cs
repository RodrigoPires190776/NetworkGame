using Network.RouteDiscovery;
using Network.Strategies;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using Network.UpdateNetwork;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Network.RouteDiscovery.BaseRouteDiscovery;
using static Network.Strategies.BaseStrategy;

namespace NetworkGameBackend
{
    public class Game
    {
        public Network.Network Network { get; } 
        public bool IsRunning { get; private set; }
        public event EventHandler<UpdatedState> GameStep;
        public int LoopsPerSecond { get; private set; }
        private TimeSpan LoopTime { get; set; }
        private readonly List<int> GameSpeeds = new List<int>() { 1, 2, 5, 10, 25, 50, 75, 100, 150, 250, 500, int.MaxValue };
        private int CurrentSpeed;

        public Game(Network.Network network, int speed,
            Tuple<RoutingStrategies, Dictionary<string, Property>> routing,
            Tuple<PickingStrategies, Dictionary<string, Property>> picking,
            Tuple<CreationStrategies, Dictionary<string, Property>> creation,
            Tuple<RouteDiscoveryStrategies, Dictionary<string, Property>> discovery)
        {
            Network = network;
            foreach(var router in network.RouterIDList)
            {
                Network.SetStrategies(router, GetStrategy(routing.Item1, router, routing.Item2), 
                    GetStrategy(creation.Item1, creation.Item2), 
                    GetStrategy(picking.Item1, picking.Item2));
            }
            Network.Initialize(GetStrategy(discovery.Item1, discovery.Item2));
            LoopTime = new TimeSpan(TimeSpan.TicksPerSecond / GameSpeeds[speed]);
            CurrentSpeed = speed;
        }

        public void Run()
        {
            int loopCounter = 0;
            TimeSpan loopsTimeCounter = new TimeSpan(0);
            TimeSpan elapsedTime;
            DateTime startLoopTime;
            IsRunning = true;
            while (IsRunning)
            {
                startLoopTime = DateTime.Now;

                GameStep.Invoke(this, Network.Step());

                elapsedTime = DateTime.Now - startLoopTime;
                if (elapsedTime < LoopTime) System.Threading.Thread.Sleep(LoopTime - elapsedTime);

                loopsTimeCounter += DateTime.Now - startLoopTime;
                loopCounter++;
                if (loopsTimeCounter > TimeSpan.FromSeconds(1))
                {
                    LoopsPerSecond = loopCounter;
                    loopCounter = 0;
                    loopsTimeCounter = new TimeSpan(0);
                }
            }
            LoopsPerSecond = 0;
        }

        public void Pause()
        {
            LoopsPerSecond = 0;
            IsRunning = false;
        }

        public void IntroduceAttacker(Guid defensorID, Guid destinationID, Guid attackerID)
        {
            Network.IntroduceAttacker(defensorID, destinationID, attackerID);
        }

        public void ChangeSpeed(int speed)
        {
            var newSpeed = CurrentSpeed + speed;
            if (newSpeed < 0 || newSpeed > GameSpeeds.Count - 1) return;

            LoopTime = new TimeSpan(TimeSpan.TicksPerSecond / GameSpeeds[newSpeed]);
            CurrentSpeed = newSpeed;
        }

        private RoutingStrategy GetStrategy(RoutingStrategies strat, Guid routerID, Dictionary<string, Property> properties)
        {
            switch (strat)
            {
                case RoutingStrategies.Random:
                    return new RandomRoutingStrategy(routerID, Network.ID, properties);
                case RoutingStrategies.LinearRewardInaction:
                    return new LinearRewardInactionRoutingStrategy(routerID, Network.ID, properties);
                case RoutingStrategies.LinearRewardPenalty:
                    return new LinearRewardPenaltyRoutingStrategy(routerID, Network.ID, properties);
                default:
                    throw new Exception("Invalid routing strategy");
            }
        }

        private PacketPickingStrategy GetStrategy(PickingStrategies strat, Dictionary<string, Property> properties)
        {
            switch (strat)
            {
                case PickingStrategies.Random:
                    return new RandomPacketPickingStrategy(Network.ID, properties);
                default:
                    throw new Exception("Invalid packet picking strategy");
            }
        }

        private PacketCreationStrategy GetStrategy(CreationStrategies strat, Dictionary<string, Property> properties)
        {
            switch (strat)
            {
                case CreationStrategies.Random:
                    return new RandomPacketCreationStrategy(Network.ID, properties);
                default:
                    throw new Exception("Invalid packet creation strategy");
            }
        }

        private BaseRouteDiscovery GetStrategy(RouteDiscoveryStrategies strat, Dictionary<string, Property> properties)
        {
            switch (strat)
            {
                case RouteDiscoveryStrategies.None:
                    return new NoneRouteDiscovery(Network);
                case RouteDiscoveryStrategies.BreadthFirstRouteDiscovery:
                    return new BreadthFirstRouteDiscovery(Network);
                case RouteDiscoveryStrategies.DijkstraRouteDiscovery:
                    return new DijkstraRouteDiscovery(Network, properties);
                case RouteDiscoveryStrategies.BestRouteOnlyDiscovery:
                    return new BestRouteOnlyDiscovery(Network, properties);
                default:
                    throw new Exception("Invalid route discovery strategy");
            }
        }
    }
    
}
