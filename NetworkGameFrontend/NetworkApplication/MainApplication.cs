using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameFrontend.VisualData;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualNetwork;
using NetworkGenerator.NetworkExporter;
using NetworkGenerator.NetworkImporter.NetworkFile;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static Network.RouteDiscovery.BaseRouteDiscovery;
using static Network.Strategies.BaseStrategy;
using static NetworkGameFrontend.VisualData.Options.Base.BasePlot;

namespace NetworkGameFrontend.NetworkApplication
{
    public class MainApplication
    {
        private readonly NetworkViewerController NetworkViewerController;
        private readonly Grid NetworkControlsGrid;
        private readonly Grid NetworkViewerSettingsGrid;
        private DispatcherTimer FPSCounterTimer;
        private int NumberOfFrames = 0;
        private int TotalNumberOfCycles = 0;
        public Guid LoadedNetwork { get; private set; }
        public string LoadedNetworkName { 
            get 
            {
                return NetworkMaster.GetInstance().GetNetwork(LoadedNetwork).Name;
            } 
        }
        public int NumberOfGames
        {
            get
            {
                return GameMaster.GetInstance().NumberOfGames;
            }
        }
        private Guid LoadedRouter;
        private Game LoadedNetworkGame;
        private NetworkUpdateStateQueue NetworkUpdateStateQueue;
        private readonly object _UpdateLock = new object();

        public MainApplication(Grid networkViewer, Grid networkControlsGrid)
        {
            NetworkViewerController = new NetworkViewerController(
                (ScrollViewer)networkViewer.FindName("NetworkScrollViewer"),
                (Slider)networkViewer.FindName("NetworkScrollViewerSlider"),
                (Grid)networkViewer.FindName("NetworkScrollViewerGrid"),
                (ScaleTransform)networkViewer.FindName("NetworkScrollViewerGridScaleTransform"),
                (ContentPresenter)networkViewer.FindName("NetworkScrollViewerContent")
                );
            NetworkViewerSettingsGrid = (Grid)networkViewer.FindName("NetworkViewerSettingsGrid");
            NetworkControlsGrid = networkControlsGrid;
            LoadedRouter = Guid.Empty;
        }
        public NetworkInfo ImportNetwork(Stream fileStream, string networkName)
        {
            var importer = new NetworkFileImporter();
            var network = importer.Import(fileStream, 5);
            network.ComputeRouterDistances();
            NetworkMaster.GetInstance().AddNetwork(network, networkName);
            NetworkDataCollector.GetInstance().AddNetwork(network);

            return network.GetNetworkInfo();
        }

        public NetworkInfo GenerateNetwork(Dictionary<string, Property> properties)
        {
            var generator = new NetworkGenerator.Generator.NetworkGenerator();
            var network = generator.GenerateNetwork(properties);
            network.ComputeRouterDistances();
            NetworkMaster.GetInstance().AddNetwork(network, "GeneratedNetwork");
            NetworkDataCollector.GetInstance().AddNetwork(network);

            return network.GetNetworkInfo();
        }

        public void LoadNetwork(string networkName)
        {
            LoadedNetwork = NetworkMaster.GetInstance().GetNetworkByName(networkName).ID;
            NetworkViewerController.LoadNetwork(LoadedNetwork, LoadedRouter);         
            foreach(var router in NetworkViewerController.VisualNetwork.Routers.Values)
            {
                router.ClickedRouter += UpdatedClickedRouter;
            }
        }

        public void LoadNetwork(int gameID)
        {
            LoadedNetworkGame = GameMaster.GetInstance().GetGame(gameID);
            LoadedNetwork = LoadedNetworkGame.Network.ID;
            var x = NetworkViewerController.VisualNetwork;
            var visualNetwork = NetworkViewerController.LoadNetwork(LoadedNetwork, LoadedRouter);
            foreach (var router in visualNetwork.Item1.Routers.Values)
            {
                router.ClickedRouter += UpdatedClickedRouter;
            }
            ((TextBlock)NetworkViewerSettingsGrid.FindName("LoadedGameIDTextbox")).Text = gameID.ToString();
            if(LoadedRouter != Guid.Empty)
            {
                LoadedRouter = visualNetwork.Item2;
                UpdatedClickedRouter(null, new ClickedRouterEventArgs(LoadedRouter));
            }
        }

        public void ExportNetwork(string filePath)
        {
            var exporter = new NetworkExporter();
            exporter.Export(NetworkMaster.GetInstance().GetNetwork(LoadedNetwork), filePath);
        }

        public List<string> GetAllNetworksName()
        {
            return NetworkMaster.GetInstance().GetAllNetworkNames();
        }

        public int GetNumberOfRouters()
        {
            return NetworkMaster.GetInstance().GetNetwork(LoadedNetwork).RouterIDList.Count;
        }

        public void StartDiscovery(int numberOfGames, int ttl, bool? saveRuntimeData,
            Tuple<RoutingStrategies, Dictionary<string, Property>> routingStrategy, 
            Tuple<PickingStrategies, Dictionary<string, Property>> pickingStrategy, 
            Tuple<CreationStrategies, Dictionary<string, Property>> creationStrategy,
            Tuple<RouteDiscoveryStrategies, Dictionary<string, Property>> discoveryStrategy)
        {
            NetworkMaster.PacketTTL = ttl;
            TotalNumberOfCycles = 0;
            NetworkUpdateStateQueue = new NetworkUpdateStateQueue();
            NetworkDataCollector.GetInstance().SaveRuntimeData(saveRuntimeData);

            var games = GameMaster.GetInstance().StartGames(numberOfGames, 0, NetworkMaster.GetInstance().GetNetwork(LoadedNetwork),
                routingStrategy, pickingStrategy, creationStrategy, discoveryStrategy);
            LoadedNetworkGame = GameMaster.GetInstance().GetGame(1);
            LoadNetwork(1);
            // LoadedNetwork = GameMaster.GetInstance().GetGame(1).Network.ID;
            //NetworkViewerController.LoadNetwork(LoadedNetwork);
            foreach (var game in games)
            {
                game.GameStep += UpdateNetwork;
                NetworkDataCollector.GetInstance().AddNetwork(game.Network);
                NetworkDataCollector.GetInstance().AddEventHandler(game.Network.ID, game);
            }

            //Thread t = new Thread(GameMaster.GetInstance().Run);
            //t.Start();
            FPSCounterTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            FPSCounterTimer.Tick += UpdateFPSCounter;
            FPSCounterTimer.Start();
            EnableNetworkViewerControls();
        }

        public Dictionary<string, Property> GetIntroduceAttackerProperties()
        {
            var properties = new Dictionary<string, Property>
            {
                {
                    Property.Attacker,
                    new Property(Property.PropertyType.Integer, new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0),
                        new Tuple<string, object>(Property.INTEGER_MAX, NetworkMaster.GetInstance().GetNetwork(LoadedNetwork).Routers.Count - 1)
                    })
                },
                {
                    Property.Defensor,
                    new Property(Property.PropertyType.Integer, new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0),
                        new Tuple<string, object>(Property.INTEGER_MAX, NetworkMaster.GetInstance().GetNetwork(LoadedNetwork).Routers.Count - 1)
                    })
                },
                {
                    Property.Destination,
                    new Property(Property.PropertyType.Integer, new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0),
                        new Tuple<string, object>(Property.INTEGER_MAX, NetworkMaster.GetInstance().GetNetwork(LoadedNetwork).Routers.Count - 1)
                    })
                },
                {
                    Property.Random,
                    new Property(Property.PropertyType.Bool, new List<Tuple<string, object>>())
                }
            };

            return properties;
        }

        public void IntroduceAttacker(int defensor, int destination, int attacker, bool random)
        {
            var defensorID = NetworkViewerController.VisualNetwork.RouterIDs[defensor];
            var destinationID = NetworkViewerController.VisualNetwork.RouterIDs[destination];
            var attackerID = NetworkViewerController.VisualNetwork.RouterIDs[attacker];

            GameMaster.GetInstance().IntroduceAttacker(defensorID, destinationID, attackerID, random);
            var agents = NetworkMaster.GetInstance().GetNetwork(LoadedNetwork).GetNetworkAgents();
            NetworkViewerController.VisualNetwork.IntroduceAttacker(agents.Item1, agents.Item2, agents.Item3);
        }

        private void UpdateNetwork(object sender, UpdatedState eventArgs)
        {
            NetworkUpdateStateQueue.AddState(eventArgs);
            lock (_UpdateLock)
            {
                var state = NetworkUpdateStateQueue.GetState(LoadedNetwork);
                if (state != null)
                {
                    NetworkViewerController.Update(state, LoadedRouter);
                    if (LoadedRouter != Guid.Empty) UpdateRouterData(LoadedRouter);
                    UpdateNetworkData(state);
                    NumberOfFrames++;
                }
                else
                {
                    Debug.WriteLine("Skipped frame");
                }
            }           
        }

        private void UpdatedClickedRouter(object sender, ClickedRouterEventArgs eventArgs)
        {
            var tBox = NetworkControlsGrid.FindName("RouterNumberTextBox") as TextBox;
            tBox.Text = NetworkViewerController.VisualNetwork.Routers[eventArgs.ID].ID.ToString();
            LoadedRouter = eventArgs.ID;
            UpdateRouterData(eventArgs.ID);
        }

        private void UpdateRouterData(Guid routerID)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    var routerData = NetworkDataCollector.GetInstance().GetRouterData(LoadedNetwork, routerID);
                    var tBox = NetworkControlsGrid.FindName("PacketsCreatedTextBox") as TextBox;
                    tBox.Text = routerData.PacketsCreated.ToString();
                    tBox = NetworkControlsGrid.FindName("PacketsSentTextBox") as TextBox;
                    tBox.Text = routerData.PacketsSent.ToString();
                    tBox = NetworkControlsGrid.FindName("PacketsDeliveredTextBox") as TextBox;
                    tBox.Text = routerData.PacketsDelivered.ToString();
                    tBox = NetworkControlsGrid.FindName("PacketsDroppedTextBox") as TextBox;
                    tBox.Text = routerData.PacketsDropped.ToString();
                    tBox = NetworkControlsGrid.FindName("PacketsInTransitTextBox") as TextBox;
                    tBox.Text = routerData.PacketsInTransit.ToString();
                    tBox = NetworkControlsGrid.FindName("PacketsInQueueTextBox") as TextBox;
                    tBox.Text = routerData.PacketsInQueue.ToString();
                    tBox = NetworkControlsGrid.FindName("PacketsReceivedTextBox") as TextBox;
                    tBox.Text = routerData.PacketsReceived.ToString();
                    tBox = NetworkControlsGrid.FindName("PacketAverageDeliverTimeTextBox") as TextBox;
                    tBox.Text = routerData.PacketAverageDeliverTime.ToString();
                });
            NetworkViewerController.UpdateRouterData(routerID);
        }

        private void UpdateNetworkData(UpdatedState state)
        {
            Application.Current.Dispatcher.Invoke(
               () =>
               {
                   ((TextBox)NetworkControlsGrid.FindName("AverageVarianceTextBox")).Text = state.AverageVarience.ToString();
               });       
        }

        private void UpdateFPSCounter(object sender, object eventArgs)
        {
            ((TextBlock)NetworkViewerSettingsGrid.FindName("FPSCounter")).Text = NumberOfFrames.ToString();
            NumberOfFrames = 0;
            ((TextBlock)NetworkViewerSettingsGrid.FindName("LoopCounter")).Text = GameMaster.GetInstance().LoopsPerSecond.ToString();
            TotalNumberOfCycles += GameMaster.GetInstance().LoopsPerSecond;
            ((TextBlock)NetworkViewerSettingsGrid.FindName("TotalLoopCounter")).Text = TotalNumberOfCycles.ToString();
        }

        private void EnableNetworkViewerControls()
        {
            ((Button)NetworkViewerSettingsGrid.FindName("NetworkViewerSlowDown")).IsEnabled = true;
            ((Button)NetworkViewerSettingsGrid.FindName("NetworkViewerStartPause")).IsEnabled = true;
            //((Button)NetworkViewerSettingsGrid.FindName("NetworkViewerStartPause")).Content = "\uE769";
            ((Button)NetworkViewerSettingsGrid.FindName("NetworkViewerSpeedUp")).IsEnabled = true;
            ((Button)NetworkViewerSettingsGrid.FindName("NetworkViewerUpdatePackets")).IsEnabled = true;
        }

        public void ViewerStartPause()
        {
            if (GameMaster.GetInstance().IsRunning)
            {
                TotalNumberOfCycles += GameMaster.GetInstance().LoopsPerSecond;
                GameMaster.GetInstance().Pause();
                Application.Current.Dispatcher.Invoke(
                () =>
                {
                    ((Button)NetworkViewerSettingsGrid.FindName("NetworkViewerStartPause")).Content = "\uE102";                    
                    ((TextBlock)NetworkViewerSettingsGrid.FindName("TotalLoopCounter")).Text = TotalNumberOfCycles.ToString();
                });                    
            }
            else
            {
                Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Thread t = new Thread(GameMaster.GetInstance().Run);
                    t.Start();
                    ((Button)NetworkViewerSettingsGrid.FindName("NetworkViewerStartPause")).Content = "\uE769";
                });
            }
        }

        public void GameSpeedChange(int speed)
        {
            GameMaster.GetInstance().ChangeSpeed(speed);
        }

        public bool ToggleUpdatePackets()
        {
            NetworkViewerController.UpdatePackets = !NetworkViewerController.UpdatePackets;
            return NetworkViewerController.UpdatePackets;
        }
        public BasePlot GetPlot(PlotType type)
        {
            var plot = PlotFactory.GetPlot(type, LoadedNetwork, LoadedNetworkGame,
                GameMaster.GetInstance().GetNetworkList(), GameMaster.GetInstance().GetGamesList());
            return plot;
        }

        public (BasePlot,int) InitializePlot(BasePlot plot, Dictionary<string, Property> properties)
        {
            return (plot.Initialize(NetworkViewerController.VisualNetwork, properties),NetworkViewerController.VisualNetwork.NetworkID);
        }
    }
}
