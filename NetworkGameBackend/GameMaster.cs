using Network.UpdateNetwork;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Network.RouteDiscovery.BaseRouteDiscovery;
using static Network.Strategies.BaseStrategy;

namespace NetworkGameBackend
{
    public class GameMaster
    {
        private const int MAX_NUMBER_OF_TASKS = 32;
        public event EventHandler<List<UpdatedState>> AllGamesStep;
        public int NumberOfGames 
        {
            get
            {
                return Games != null ? Games.Keys.Count : 0;
            }
        }
        private static readonly object InstanceLock = new object();
        private static GameMaster Instance;
        private Dictionary<int, Game> Games;
        private Network.Network OriginalNetwork;
        public bool IsRunning { get; private set; }
        public int LoopsPerSecond { get; private set; }
        private TimeSpan LoopTime { get; set; }
        private readonly List<int> GameSpeeds = new List<int>() { 1, 2, 5, 10, 25, 50, 75, 100, 150, 250, 500, int.MaxValue };
        private int CurrentSpeed;
        private GameMaster()
        {
            Instance = this;
            Games = new Dictionary<int, Game>();
        }

        public static GameMaster GetInstance()
        {
            if (Instance == null)
            {
                lock (InstanceLock)
                {
                    if (Instance == null) Instance = new GameMaster();
                }
            }
            return Instance;
        }

        public Game GetGame(int id)
        {
            if (Games.ContainsKey(id)) return Games[id];
            throw new Exception($"Game with id {id} doesn't exist!");
        }

        public List<Game> StartGames(int numberOfGames, int speed,
            Network.Network network,
            Tuple<RoutingStrategies, Dictionary<string, Property>> routingStrategy,
            Tuple<PickingStrategies, Dictionary<string, Property>> pickingStrategy,
            Tuple<CreationStrategies, Dictionary<string, Property>> creationStrategy,
            Tuple<RouteDiscoveryStrategies, Dictionary<string, Property>> discoveryStrategy)
        {
            LoopTime = new TimeSpan(TimeSpan.TicksPerSecond / GameSpeeds[speed]);
            CurrentSpeed = speed;
            OriginalNetwork = network;
            var games = new List<Game>();

            for (int i = 1; i <= numberOfGames; i++)
            {
                var networkCopy = network.Copy();
                Games.Add(i, new Game(networkCopy, 5, routingStrategy, pickingStrategy, creationStrategy, discoveryStrategy));
                games.Add(Games[i]);

            }
            return games;
        }

        private List<List<Game>> GetGamesPerTask()
        {
            List<List<Game>> games = new List<List<Game>>(MAX_NUMBER_OF_TASKS);
            for (int i = 0; i < MAX_NUMBER_OF_TASKS; i++) games.Add(new List<Game>());

            decimal numberOfGamesPerTask = Games.Keys.Count / MAX_NUMBER_OF_TASKS;

            var listIndex = 0;
            foreach(var game in Games.Values)
            {
                games[listIndex].Add(game);
                listIndex = listIndex >= MAX_NUMBER_OF_TASKS - 1 ? 0 : listIndex + 1;
            }

            return games;
        }

        public void Run()
        {
            int loopCounter = 0;
            TimeSpan loopsTimeCounter = new TimeSpan(0);
            TimeSpan elapsedTime;
            DateTime startLoopTime;
            List<Task> tasks = new List<Task>();
            List<UpdatedState> states = new List<UpdatedState>();
            IsRunning = true;
            while (IsRunning)
            {
                startLoopTime = DateTime.Now;
                tasks.Clear();
                states.Clear();

                bool startTask = false;
                var listGames = GetGamesPerTask();
                foreach(var games in listGames)
                {
                    if (games.Count == 0) break;
                    var list = new List<Game>();

                    Task<List<UpdatedState>> t = Task.Run(() => 
                    {
                        var updateStates = new List<UpdatedState>();
                        foreach (var game in games)
                        {
                            updateStates.Add(game.Loop());
                        }
                        return updateStates; 
                    });
                    
                    tasks.Add(t);
                }
                foreach(Task<List<UpdatedState>> task in tasks)
                {
                    task.Wait();
                    states.AddRange(task.Result);
                }
                AllGamesStep?.Invoke(this, states);

                elapsedTime = DateTime.Now - startLoopTime;
                if (elapsedTime < LoopTime) Thread.Sleep(LoopTime - elapsedTime);

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

        public void IntroduceAttacker(Guid defensorID, Guid destinationID, Guid attackerID, bool random)
        {
            if (random)
            {
                foreach (var game in Games.Values)
                {
                    game.Network.IntroduceAttackerRandom();
                }
            }
            else
            {
                foreach (var game in Games.Values)
                {
                    game.Network.IntroduceAttacker(defensorID, destinationID, attackerID);
                }
            }                   
        }

        public void ChangeSpeed(int speed)
        {
            var newSpeed = CurrentSpeed + speed;
            if (newSpeed < 0 || newSpeed > GameSpeeds.Count - 1) return;

            LoopTime = new TimeSpan(TimeSpan.TicksPerSecond / GameSpeeds[newSpeed]);
            CurrentSpeed = newSpeed;
        }

        public List<Guid> GetNetworkList()
        {
            var list = new List<Guid>();
            foreach(var game in Games.Values)
            {
                list.Add(game.Network.ID);
            }
            return list;
        }

        public List<Game> GetGamesList()
        {
            var list = new List<Game>();
            foreach (var game in Games.Values)
            {
                list.Add(game);
            }
            return list;
        }
    }
}
