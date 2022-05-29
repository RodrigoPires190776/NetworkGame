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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NetworkGameFrontend.NetworkApplication
{
    public class MainApplication
    {
        private NetworkViewerController NetworkViewerController;
        private Grid NetworkControlsGrid;
        private Guid LoadedNetwork;
        private Guid LoadedRouter;
        private Game NetworkGame;

        public MainApplication(ScrollViewer networkScrollViewer, Grid networkControlsGrid)
        {
            NetworkViewerController = new NetworkViewerController(networkScrollViewer);
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
            NetworkGame = new Game(NetworkMaster.GetInstance().GetNetwork(LoadedNetwork), 10, 
                RoutingStrategies.Random, PickingStrategies.Random, CreationStrategies.Random);
            NetworkGame.GameStep += UpdateNetwork;
            NetworkDataCollector.GetInstance().AddEventHandler(LoadedNetwork, NetworkGame);
            Thread t = new Thread(NetworkGame.Run);
            t.Start();
        }

        private void UpdateNetwork(object sender, UpdatedState eventArgs)
        {
            Debug.WriteLine("updated");
            NetworkViewerController.Update(eventArgs);
            if (LoadedRouter != Guid.Empty) UpdateRouterData(LoadedRouter);
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
        }
    }
}
