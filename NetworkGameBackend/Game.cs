using System;
using System.Threading.Tasks;

namespace NetworkGameBackend
{
    public class Game
    {
        public Network.Network Network { get; } 
        public bool IsRunning { get; private set; }
        public EventHandler GameStep { get; }
        public int LoopsPerSecond { get; private set; }
        private TimeSpan LoopTime { get; set; }

        public Game(Network.Network network, int numberLoopsPerSecond)
        {
            Network = network;
            LoopTime = new TimeSpan(TimeSpan.TicksPerSecond / numberLoopsPerSecond);
        }

        public async void Run()
        {
            int loopCounter = 0;
            TimeSpan loopsTimeCounter = new TimeSpan(0);
            TimeSpan elapsedTime;
            DateTime startLoopTime; 
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
    }
}
