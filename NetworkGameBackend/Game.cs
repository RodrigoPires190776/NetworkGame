using Network.Strategies;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using Network.UpdateNetwork;
using System;
using System.Threading.Tasks;

namespace NetworkGameBackend
{
    public class Game
    {
        public Network.Network Network { get; } 
        public bool IsRunning { get; private set; }
        public event EventHandler<UpdatedState> GameStep;
        public int LoopsPerSecond { get; private set; }
        private TimeSpan LoopTime { get; set; }

        public Game(Network.Network network, int numberLoopsPerSecond,
            RoutingStrategies routing, PickingStrategies packetPicking, CreationStrategies packetCreation)
        {
            Network = network;
            foreach(var router in network.RouterIDList)
            {
                Network.SetStrategies(router, GetStrategy(routing), GetStrategy(packetCreation), GetStrategy(packetPicking));
            }
            LoopTime = new TimeSpan(TimeSpan.TicksPerSecond / numberLoopsPerSecond);
        }

        public async void Run()
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
                if(elapsedTime < LoopTime) await Task.Delay(LoopTime - elapsedTime);

                loopsTimeCounter += DateTime.Now - startLoopTime;
                loopCounter++;
                if (loopsTimeCounter > TimeSpan.FromSeconds(1))
                {
                    LoopsPerSecond = loopCounter;
                    loopCounter = 0;
                    loopsTimeCounter = new TimeSpan(0);
                }
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        private RoutingStrategy GetStrategy(RoutingStrategies strat)
        {
            switch (strat)
            {
                case RoutingStrategies.Random:
                    return new RandomRoutingStrategy();
                default:
                    throw new Exception("Invalid routing strategy");
            }
        }

        private PacketPickingStrategy GetStrategy(PickingStrategies strat)
        {
            switch (strat)
            {
                case PickingStrategies.Random:
                    return new RandomPacketPickingStrategy();
                default:
                    throw new Exception("Invalid packet picking strategy");
            }
        }

        private PacketCreationStrategy GetStrategy(CreationStrategies strat)
        {
            switch (strat)
            {
                case CreationStrategies.Random:
                    return new RandomPacketCreationStrategy();
                default:
                    throw new Exception("Invalid packet creation strategy");
            }
        }
    }
    public enum RoutingStrategies { Random };
    public enum PickingStrategies { Random };
    public enum CreationStrategies { Random };
}
