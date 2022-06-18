using Network.Strategies;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using Network.UpdateNetwork;
using System;
using System.Collections.Generic;
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
        private readonly List<int> GameSpeeds = new List<int>() { 1, 2, 5, 10, 25, 50, 75, 100, 150, 250, 500};
        private int CurrentSpeed;

        public Game(Network.Network network, int speed,
            RoutingStrategies routing, PickingStrategies packetPicking, CreationStrategies packetCreation)
        {
            Network = network;
            foreach(var router in network.RouterIDList)
            {
                Network.SetStrategies(router, GetStrategy(routing, router), GetStrategy(packetCreation), GetStrategy(packetPicking));
            }
            LoopTime = new TimeSpan(TimeSpan.TicksPerSecond / GameSpeeds[speed]);
            CurrentSpeed = speed;
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
                if(elapsedTime < LoopTime) System.Threading.Thread.Sleep(LoopTime - elapsedTime);

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

        private RoutingStrategy GetStrategy(RoutingStrategies strat, Guid routerID)
        {
            switch (strat)
            {
                case RoutingStrategies.Random:
                    return new RandomRoutingStrategy(routerID, Network.ID);
                case RoutingStrategies.LinearRewardInaction:
                    return new LinearRewardInactionRoutingStrategy(routerID, Network.ID);
                default:
                    throw new Exception("Invalid routing strategy");
            }
        }

        private PacketPickingStrategy GetStrategy(PickingStrategies strat)
        {
            switch (strat)
            {
                case PickingStrategies.Random:
                    return new RandomPacketPickingStrategy(Network.ID);
                default:
                    throw new Exception("Invalid packet picking strategy");
            }
        }

        private PacketCreationStrategy GetStrategy(CreationStrategies strat)
        {
            switch (strat)
            {
                case CreationStrategies.Random:
                    return new RandomPacketCreationStrategy(Network.ID);
                default:
                    throw new Exception("Invalid packet creation strategy");
            }
        }
    }
    public enum RoutingStrategies { Random, LinearRewardInaction };
    public enum PickingStrategies { Random };
    public enum CreationStrategies { Random };
}
