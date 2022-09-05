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
        public event EventHandler<UpdatedState> GameStep;

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
        }

        public UpdatedState Loop()
        {
            var state = Network.Step();
            GameStep.Invoke(this, state);
            return state;
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
