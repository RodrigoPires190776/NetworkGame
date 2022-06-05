using Network;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameFrontend.VisualNetwork;
using NetworkGenerator.NetworkImporter.NetworkFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NetworkGameFrontend.NetworkApplication
{
    public class MainApplication
    {
        private NetworkViewerController NetworkViewerController;
        private Grid NetworkControlsGrid;
        private Grid NetworkFPSCounter;
        private DispatcherTimer FPSCounterTimer;
        private int NumberOfFrames = 0;
        private Guid LoadedNetwork;
        private Guid LoadedRouter;
        private Game NetworkGame;
        private NetworkUpdateStateQueue NetworkUpdateStateQueue;
        private object _UpdateLock = new object();

        public MainApplication(Grid networkViewer, Grid networkControlsGrid)
        {
            NetworkViewerController = new NetworkViewerController((ScrollViewer)networkViewer.FindName("NetworkScrollViewer"));
            NetworkFPSCounter = (Grid)networkViewer.FindName("FPSCounterGrid");
            NetworkControlsGrid = networkControlsGrid;
            LoadedRouter = Guid.Empty;
        }
        public void ImportNetwork(Stream fileStream, string networkName)
        {
            var importer = new NetworkFileImporter();
            var network = importer.Import(fileStream, 10);

            NetworkMaster.GetInstance().AddNetwork(network, networkName);
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

        public void StartDiscovery()
        {
            //TODO choose strategies
            NetworkGame = new Game(NetworkMaster.GetInstance().GetNetwork(LoadedNetwork), 5, 
                RoutingStrategies.LinearRewardInaction, PickingStrategies.Random, CreationStrategies.Random);
            NetworkUpdateStateQueue = new NetworkUpdateStateQueue();
            NetworkGame.GameStep += UpdateNetwork;
            NetworkDataCollector.GetInstance().AddEventHandler(LoadedNetwork, NetworkGame);
            Thread t = new Thread(NetworkGame.Run);
            t.Start();
            FPSCounterTimer = new DispatcherTimer();
            FPSCounterTimer.Interval = TimeSpan.FromSeconds(1);
            FPSCounterTimer.Tick += UpdateFPSCounter;
            FPSCounterTimer.Start();
            EnableNetworkViewerControls();
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
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
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

        private void UpdateFPSCounter(object sender, object eventArgs)
        {
            ((TextBlock)NetworkFPSCounter.FindName("FPSCounter")).Text = NumberOfFrames.ToString();
            NumberOfFrames = 0;
            ((TextBlock)NetworkFPSCounter.FindName("LoopCounter")).Text = NetworkGame.LoopsPerSecond.ToString();
        }

        private void EnableNetworkViewerControls()
        {
            ((Button)NetworkFPSCounter.FindName("NetworkViewerSlowDown")).IsEnabled = true;
            ((Button)NetworkFPSCounter.FindName("NetworkViewerStartPause")).IsEnabled = true;
            ((Button)NetworkFPSCounter.FindName("NetworkViewerStartPause")).Content = "\uE769";
            ((Button)NetworkFPSCounter.FindName("NetworkViewerSpeedUp")).IsEnabled = true;
        }

        public void ViewerStartPause()
        {
            if (NetworkGame.IsRunning)
            {
                NetworkGame.Pause();
                _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    NetworkGame.Pause();
                    ((Button)NetworkFPSCounter.FindName("NetworkViewerStartPause")).Content = "\uE102";
                });                    
            }
            else
            {
                _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
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
    }
}
