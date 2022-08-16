﻿using Network;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameFrontend.VisualData;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualNetwork;
using NetworkGenerator.NetworkImporter.NetworkFile;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly Grid NetworkFPSCounter;
        private DispatcherTimer FPSCounterTimer;
        private int NumberOfFrames = 0;
        private int TotalNumberOfCycles = 0;
        public Guid LoadedNetwork { get; private set; }
        private Guid LoadedRouter;
        private Game NetworkGame;
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
            NetworkFPSCounter = (Grid)networkViewer.FindName("FPSCounterGrid");
            NetworkControlsGrid = networkControlsGrid;
            LoadedRouter = Guid.Empty;
        }
        public void ImportNetwork(Stream fileStream, string networkName)
        {
            var importer = new NetworkFileImporter();
            var network = importer.Import(fileStream, 5);

            NetworkMaster.GetInstance().AddNetwork(network, networkName);
            NetworkDataCollector.GetInstance().AddNetwork(network);
        }

        public void GenerateNetwork(Dictionary<string, Property> properties)
        {
            var generator = new NetworkGenerator.Generator.NetworkGenerator();
            var network = generator.GenerateNetwork(properties);

            NetworkMaster.GetInstance().AddNetwork(network, "GeneratedNetwork");
            NetworkDataCollector.GetInstance().AddNetwork(network);
        }

        public void LoadNetwork(string networkName)
        {
            NetworkViewerController.LoadNetwork(networkName);
            LoadedNetwork = NetworkMaster.GetInstance().GetNetworkByName(networkName).ID;
            foreach(var router in NetworkViewerController.VisualNetwork.Routers.Values)
            {
                router.ClickedRouter += UpdatedClickedRouter;
            }
        }

        public List<string> GetAllNetworksName()
        {
            return NetworkMaster.GetInstance().GetAllNetworkNames();
        }

        public int GetNumberOfRouters()
        {
            return NetworkMaster.GetInstance().GetNetwork(LoadedNetwork).RouterIDList.Count;
        }

        public void StartDiscovery(Tuple<RoutingStrategies, Dictionary<string, Property>> routingStrategy, 
            Tuple<PickingStrategies, Dictionary<string, Property>> pickingStrategy, 
            Tuple<CreationStrategies, Dictionary<string, Property>> creationStrategy,
            Tuple<RouteDiscoveryStrategies, Dictionary<string, Property>> discoveryStrategy)
        {
            TotalNumberOfCycles = 0;
            NetworkGame = new Game(NetworkMaster.GetInstance().GetNetwork(LoadedNetwork), 5,
                routingStrategy, pickingStrategy, creationStrategy, discoveryStrategy);
            NetworkUpdateStateQueue = new NetworkUpdateStateQueue();
            NetworkGame.GameStep += UpdateNetwork;
            NetworkDataCollector.GetInstance().AddEventHandler(LoadedNetwork, NetworkGame);
            Thread t = new Thread(NetworkGame.Run);
            t.Start();
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
                }
            };

            return properties;
        }

        public void IntroduceAttacker(int defensor, int destination, int attacker)
        {
            var defensorID = NetworkViewerController.VisualNetwork.RouterIDs[defensor];
            var destinationID = NetworkViewerController.VisualNetwork.RouterIDs[destination];
            var attackerID = NetworkViewerController.VisualNetwork.RouterIDs[attacker];

            NetworkGame.IntroduceAttacker(defensorID, destinationID, attackerID);
            NetworkViewerController.VisualNetwork.IntroduceAttacker(defensorID, destinationID, attackerID);
        }

        private void UpdateNetwork(object sender, UpdatedState eventArgs)
        {
            NetworkUpdateStateQueue.AddState(eventArgs);
            lock (_UpdateLock)
            {
                var state = NetworkUpdateStateQueue.GetState();
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
            ((TextBlock)NetworkFPSCounter.FindName("FPSCounter")).Text = NumberOfFrames.ToString();
            NumberOfFrames = 0;
            ((TextBlock)NetworkFPSCounter.FindName("LoopCounter")).Text = NetworkGame.LoopsPerSecond.ToString();
            TotalNumberOfCycles += NetworkGame.LoopsPerSecond;
            ((TextBlock)NetworkFPSCounter.FindName("TotalLoopCounter")).Text = TotalNumberOfCycles.ToString();
        }

        private void EnableNetworkViewerControls()
        {
            ((Button)NetworkFPSCounter.FindName("NetworkViewerSlowDown")).IsEnabled = true;
            ((Button)NetworkFPSCounter.FindName("NetworkViewerStartPause")).IsEnabled = true;
            ((Button)NetworkFPSCounter.FindName("NetworkViewerStartPause")).Content = "\uE769";
            ((Button)NetworkFPSCounter.FindName("NetworkViewerSpeedUp")).IsEnabled = true;
            ((Button)NetworkFPSCounter.FindName("NetworkViewerUpdatePackets")).IsEnabled = true;
        }

        public void ViewerStartPause()
        {
            if (NetworkGame.IsRunning)
            {
                TotalNumberOfCycles += NetworkGame.LoopsPerSecond;
                NetworkGame.Pause();
                Application.Current.Dispatcher.Invoke(
                () =>
                {
                    ((Button)NetworkFPSCounter.FindName("NetworkViewerStartPause")).Content = "\uE102";                    
                    ((TextBlock)NetworkFPSCounter.FindName("TotalLoopCounter")).Text = TotalNumberOfCycles.ToString();
                });                    
            }
            else
            {
                Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Thread t = new Thread(NetworkGame.Run);
                    t.Start();
                    ((Button)NetworkFPSCounter.FindName("NetworkViewerStartPause")).Content = "\uE769";
                });
            }
        }

        public void GameSpeedChange(int speed)
        {
            NetworkGame.ChangeSpeed(speed);
        }

        public bool ToggleUpdatePackets()
        {
            NetworkViewerController.UpdatePackets = !NetworkViewerController.UpdatePackets;
            return NetworkViewerController.UpdatePackets;
        }
        public BasePlot GetPlot(PlotType type)
        {
            var plot = PlotFactory.GetPlot(type, LoadedNetwork, NetworkGame);
            return plot;
        }

        public BasePlot InitializePlot(BasePlot plot, Dictionary<string, Property> properties)
        {
            return plot.Initialize(NetworkViewerController.VisualNetwork, properties);
        }
    }
}
