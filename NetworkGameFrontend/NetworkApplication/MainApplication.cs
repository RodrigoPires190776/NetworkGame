using Network;
using Network.Strategies.PacketCreation;
using Network.Strategies.PacketPicking;
using Network.Strategies.Routing;
using Network.UpdateNetwork;
using NetworkGameBackend;
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
        private Game NetworkGame;

        public MainApplication(ScrollViewer networkScrollViewer, Grid networkControlsGrid)
        {
            NetworkViewerController = new NetworkViewerController(networkScrollViewer);
            NetworkControlsGrid = networkControlsGrid;
        }
        public void ImportNetwork(Stream fileStream, string networkName)
        {
            var importer = new NetworkFileImporter();
            var network = importer.Import(fileStream, 10);

            NetworkMaster.GetInstance().AddNetwork(network, networkName);
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
                RoutingStrategies.Random, PickingStrategies.Random, CreationStrategies.Random);
            NetworkGame.GameStep += UpdateNetwork;
            Thread t = new Thread(NetworkGame.Run);
            t.Start();
        }

        private void UpdateNetwork(object sender, UpdatedState eventArgs)
        {
            Debug.WriteLine("updated");
            NetworkViewerController.Update(eventArgs);
        }

        private void UpdatedClickedRouter(object sender, ClickedRouterEventArgs eventArgs)
        {
            var routerNumber = NetworkControlsGrid.FindName("RouterNumberTextBox") as TextBox;
            routerNumber.Text = NetworkViewerController.VisualNetwork.Routers[eventArgs.ID].ID.ToString();
        }
    }
}
