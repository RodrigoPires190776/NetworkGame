using Microsoft.Win32;
using Network.RouteDiscovery;
using Network.Strategies;
using NetworkGameFrontend.ApplicationWindows;
using NetworkGameFrontend.NetworkApplication;
using NetworkGameFrontend.VisualData;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static Network.RouteDiscovery.BaseRouteDiscovery;
using static Network.Strategies.BaseStrategy;

namespace NetworkGameFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainApplication app;
        private Dictionary<string, Property> RoutingStrategyProperties; 
        private Dictionary<string, Property> PickingStrategyProperties;
        private Dictionary<string, Property> CreationStrategyProperties;
        private Dictionary<string, Property> RouteDiscoveryProperties;
        private Dictionary<string, Property> PlotProperties;
        public MainWindow()
        {
            InitializeComponent();
            app = new MainApplication(NetworkViewer, NetworkControls);
            InitializeStrategies();           
        }
        #region File
        void File_ImportNetwork_Click(object sender, RoutedEventArgs e)
        {
            var picker = new OpenFileDialog
            {
                Filter = "Network files (*.NETWORK)|*.NETWORK"
            };

            if (picker.ShowDialog() == true)
            {
                var networkNameDialog = new UserStringInput("Choose a name for the network", "Name:", picker.SafeFileName, this);
                _ =  networkNameDialog.ShowDialog();
                if (networkNameDialog.Ok)
                {
                    try
                    {
                        var stream = new FileStream(picker.FileName, FileMode.Open, FileAccess.Read);
                        app.ImportNetwork(stream, networkNameDialog.Text);
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show(ex.Message, "Something went wrong!");
                    }
                }
                else
                {
                    _ = MessageBox.Show("Import Cancelled!");
                }
            }
            else
            {
                _ = MessageBox.Show("No file or invalid file selected!");
            }
        }

        void File_GenerateNetwork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var propertiesEditor = new UserPropertyConfiguration(
                    "Network Generation Settings", 
                    NetworkGenerator.Generator.NetworkGenerator.GetProperties()
                    );
                propertiesEditor.ShowDialog();
                app.GenerateNetwork(propertiesEditor.Properties);
                _ = MessageBox.Show("NetworkGenerated!");
            }
            catch(Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region Network
        void Network_LoadNetwork_Click(object sender, RoutedEventArgs e)
        {
            var networkSelectDialog = new UserListSelectOne(app.GetAllNetworksName(), this);
            _ = networkSelectDialog.ShowDialog();

            if (networkSelectDialog.Ok)
            {
                try
                {
                    app.LoadNetwork(networkSelectDialog.Item);
                    StartDiscoveryButton.IsEnabled = true;
                    SaveRuntimeDataCheckBox.IsEnabled = true;
                    NumberOfGameTextBox.IsEnabled = true;
                    RoutingStrategyListBox.IsEnabled = true;
                    PickingStrategyListBox.IsEnabled = true;
                    CreationStrategyListBox.IsEnabled = true;
                    RouteDiscoveryListBox.IsEnabled = true;
                    InitializePlotTypes();
                    PlotTypeListBox.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.Message, "Something went wrong!");
                }
            }
            else
            {
                _ = MessageBox.Show("Load Cancelled!");
            }
        }

        void Network_ExportNetwork_Click(object sender, RoutedEventArgs e)
        {
            UserStringInput networkNameDialog;
            try
            {
                networkNameDialog = new UserStringInput("Choose a name for the network", "Name:", app.LoadedNetworkName, this);
                _ = networkNameDialog.ShowDialog();
            }
            catch(Exception ex)
            {
                _ = MessageBox.Show("No loaded network!", "Something went wrong!");
                return;
            }

            if (networkNameDialog.Ok)
            {
                try
                {
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.FileName = networkNameDialog.Text;
                    saveDialog.DefaultExt = "network";
                    saveDialog.Filter = "Network files (*.NETWORK)|*.NETWORK";

                    if (saveDialog.ShowDialog() == true)
                    {
                        var fileName = saveDialog.FileName;
                        app.ExportNetwork(fileName);
                    }
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.Message, "Something went wrong!");
                }
            }
            else
            {
                _ = MessageBox.Show("Save Cancelled!");
            }
        }
        #endregion
        #region Network Viewer Controls
        void Controls_StartDiscovery_Click(object sender, RoutedEventArgs e)
        {
            int nGames;
            if(!int.TryParse(NumberOfGameTextBox.Text, out nGames) || nGames <= 0 || nGames > 100)
            {
                _ = MessageBox.Show("Invalid number of games (1-100)!", "Something went wrong!");
            }
            var strategies = GetStrategies();
            app.StartDiscovery(nGames, SaveRuntimeDataCheckBox.IsChecked,
                               new Tuple<RoutingStrategies, Dictionary<string, Property>>(strategies.Item1, RoutingStrategyProperties), 
                               new Tuple<PickingStrategies, Dictionary<string, Property>>(strategies.Item2, PickingStrategyProperties), 
                               new Tuple<CreationStrategies, Dictionary<string, Property>>(strategies.Item3, CreationStrategyProperties),
                               new Tuple<RouteDiscoveryStrategies, Dictionary<string, Property>>(strategies.Item4, RouteDiscoveryProperties));
            StartDiscoveryButton.IsEnabled = false;
            NumberOfGameTextBox.IsEnabled = false;
            IntroduceAttackerButton.IsEnabled = true;
            PlotViewerButton.IsEnabled = true;
            NetworkViewerChangeNetwork.IsEnabled = true;
            NetworkChangeGameNumberTextbox.IsEnabled = true;
        }

        void Controls_IntroduceAttacker_Click(object sender, RoutedEventArgs e)
        {
            var properties = app.GetIntroduceAttackerProperties();

            var propertiesEditor = new UserPropertyConfiguration("IntroduceAttacker", properties);
            propertiesEditor.ShowDialog();
            
            app.IntroduceAttacker(
                (int)properties[Property.Defensor].Value, 
                (int)properties[Property.Destination].Value, 
                (int)properties[Property.Attacker].Value);
        }

        void Controls_PlotViewer_Click(object sender, RoutedEventArgs e)
        {
            var plot = app.GetPlot(BasePlot.GetPlotTypeEnum(PlotTypeListBox.Items.GetItemAt(PlotTypeListBox.SelectedIndex).ToString()));

            var initPlot = app.InitializePlot(plot, PlotProperties);

            var plotViewer = new PlotViewer(this, initPlot.Item1, initPlot.Item2, plot.AllGames);
            plotViewer.Show();
        }
        void Viewer_StartPause_Click(object sender, RoutedEventArgs e)
        {
            app.ViewerStartPause();
        }

        void Viewer_SlowDown_Click(object sender, RoutedEventArgs e)
        {
            app.GameSpeedChange(-1);
        }

        void Viewer_SpeedUp_Click(object sender, RoutedEventArgs e)
        {
            app.GameSpeedChange(1);
        }

        void Viewer_ToggleUpdatePackets_Click(object sender, RoutedEventArgs e)
        {
            app.ToggleUpdatePackets();
        }

        void Viewer_ChangeNetwork_Click(object sender, RoutedEventArgs e)
        {
            int gameID;
            if (!int.TryParse(NetworkChangeGameNumberTextbox.Text, out gameID) || gameID <= 0 || gameID > app.NumberOfGames)
            {
                _ = MessageBox.Show($"Invalid game number (1-{app.NumberOfGames})!", "Something went wrong!");
                return;
            }
            app.LoadNetwork(gameID);
        }
        #endregion
        #region Strategies
        void Controls_RoutingStrategyPropertiesEditor_Click(object sender, RoutedEventArgs e)
        {
            var propertiesEditor = new UserPropertyConfiguration(
                RoutingStrategyListBox.Items.GetItemAt(RoutingStrategyListBox.SelectedIndex) + " Properties",
                RoutingStrategyProperties);
            propertiesEditor.ShowDialog();
        }

        void Controls_UpdateRoutingStrategy(object sender, SelectionChangedEventArgs e)
        {
            RoutingStrategyProperties = BaseStrategy.GetRoutingStrategyProperties(
                RoutingStrategyListBox.Items.GetItemAt(RoutingStrategyListBox.SelectedIndex).ToString());
        }

        void Controls_PacketPickingPropertiesEditor_Click(object sender, RoutedEventArgs e)
        {
            var propertiesEditor = new UserPropertyConfiguration(
                PickingStrategyListBox.Items.GetItemAt(PickingStrategyListBox.SelectedIndex) + " Properties",
                PickingStrategyProperties);
            propertiesEditor.ShowDialog();
        }

        void Controls_PacketPickingStrategy(object sender, SelectionChangedEventArgs e)
        {
            PickingStrategyProperties = BaseStrategy.GetPickingStrategyProperties(
                PickingStrategyListBox.Items.GetItemAt(PickingStrategyListBox.SelectedIndex).ToString());
        }

        void Controls_PacketCreationPropertiesEditor_Click(object sender, RoutedEventArgs e)
        {
            var propertiesEditor = new UserPropertyConfiguration(
                CreationStrategyListBox.Items.GetItemAt(CreationStrategyListBox.SelectedIndex) + " Properties",
                CreationStrategyProperties);
            propertiesEditor.ShowDialog();
        }

        void Controls_PacketCreationStrategy(object sender, SelectionChangedEventArgs e)
        {
            CreationStrategyProperties = BaseStrategy.GetCreationStrategyProperties(
                CreationStrategyListBox.Items.GetItemAt(CreationStrategyListBox.SelectedIndex).ToString());
        }

        void Controls_RouteDiscoveryPropertiesEditor_Click(object sender, RoutedEventArgs e)
        {
            var propertiesEditor = new UserPropertyConfiguration(
                RouteDiscoveryListBox.Items.GetItemAt(RouteDiscoveryListBox.SelectedIndex) + " Properties",
                RouteDiscoveryProperties);
            propertiesEditor.ShowDialog();
        }

        void Controls_RouteDiscovery(object sender, SelectionChangedEventArgs e)
        {
            RouteDiscoveryProperties = BaseRouteDiscovery.GetRouteDiscoveryProperties(
                RouteDiscoveryListBox.Items.GetItemAt(RouteDiscoveryListBox.SelectedIndex).ToString());
        }

        private void InitializeStrategies()
        {
            RoutingStrategyListBox.ItemsSource = BaseStrategy.RoutingStrategiesList;
            RoutingStrategyListBox.SelectedIndex = 0;
            RoutingStrategyProperties = BaseStrategy.GetRoutingStrategyProperties(RoutingStrategyListBox.Items[0].ToString());

            PickingStrategyListBox.ItemsSource = BaseStrategy.PickingStrategiesList;
            PickingStrategyListBox.SelectedIndex = 0;
            PickingStrategyProperties = BaseStrategy.GetPickingStrategyProperties(PickingStrategyListBox.Items[0].ToString());

            CreationStrategyListBox.ItemsSource = BaseStrategy.CreationStrategiesList;
            CreationStrategyListBox.SelectedIndex = 0;
            CreationStrategyProperties = BaseStrategy.GetCreationStrategyProperties(CreationStrategyListBox.Items[0].ToString());

            RouteDiscoveryListBox.ItemsSource = BaseRouteDiscovery.RouteDiscoveryList;
            RouteDiscoveryListBox.SelectedIndex = 0;
            RouteDiscoveryProperties = BaseRouteDiscovery.GetRouteDiscoveryProperties(RouteDiscoveryListBox.Items[0].ToString());
        }

        private (RoutingStrategies, PickingStrategies, CreationStrategies, RouteDiscoveryStrategies) GetStrategies()
        {
            var routing = BaseStrategy.GetRoutingStrategiesEnum(RoutingStrategyListBox.SelectedItem.ToString());
            var picking = BaseStrategy.GetPickingStrategiesEnum(PickingStrategyListBox.SelectedItem.ToString());
            var creation = BaseStrategy.GetCreationStrategiesEnum(CreationStrategyListBox.SelectedItem.ToString());
            var discovery = BaseRouteDiscovery.GetRouteDiscoveryEnum(RouteDiscoveryListBox.SelectedItem.ToString());
            return (routing, picking, creation, discovery);
        }
        #endregion
        #region Plot Viewer
        private void InitializePlotTypes()
        {
            PlotTypeListBox.ItemsSource = BasePlot.PlotTypeList;
            PlotTypeListBox.SelectedIndex = 0;
            PlotProperties = BasePlot.GetPlotProperties(PlotTypeListBox.Items[0].ToString(), app.LoadedNetwork);
        }

        void Controls_PlotPropertiesEditor_Click(object sender, RoutedEventArgs e)
        {
            var propertiesEditor = new UserPropertyConfiguration(
                PlotTypeListBox.Items.GetItemAt(PlotTypeListBox.SelectedIndex) + " Properties",
                PlotProperties);
            propertiesEditor.ShowDialog();
        }

        void Controls_UpdatePlotSelected(object sender, SelectionChangedEventArgs e)
        {
            PlotProperties = BasePlot.GetPlotProperties(
                PlotTypeListBox.Items.GetItemAt(PlotTypeListBox.SelectedIndex).ToString(), app.LoadedNetwork);
        }
        #endregion
    }
}
