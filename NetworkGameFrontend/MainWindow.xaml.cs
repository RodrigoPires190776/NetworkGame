using Microsoft.Win32;
using Network.Strategies;
using NetworkGameBackend;
using NetworkGameFrontend.ApplicationWindows;
using NetworkGameFrontend.NetworkApplication;
using NetworkGameFrontend.VisualData;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Network.Strategies.BaseStrategy;
using static NetworkGameFrontend.VisualData.Options.Base.BasePlot;

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
        private Dictionary<string, Property> PlotProperties;
        public MainWindow()
        {
            InitializeComponent();
            app = new MainApplication(NetworkViewer, NetworkControls);
            InitializeStrategies();           
        }

        void File_ImportNetwork_Click(object sender, RoutedEventArgs e)
        {
            var picker = new OpenFileDialog
            {
                Filter = "Network files (*.NETWORK)|*.NETWORK"
            };

            if (picker.ShowDialog() == true)
            {
                var networkNameDialog = new UserStringInput("Choose a name for the nextwork", "Name:", picker.SafeFileName, this);
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
                app.GenerateNetwork();
                _ = MessageBox.Show("NetworkGenerated!");
            }
            catch(Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }

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
                    RoutingStrategyListBox.IsEnabled = true;
                    PickingStrategyListBox.IsEnabled = true;
                    CreationStrategyListBox.IsEnabled = true;
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

        void Controls_StartDiscovery_Click(object sender, RoutedEventArgs e)
        {
            var strategies = GetStrategies();
            app.StartDiscovery(new Tuple<RoutingStrategies, Dictionary<string, Property>>(strategies.Item1, RoutingStrategyProperties), 
                               new Tuple<PickingStrategies, Dictionary<string, Property>>(strategies.Item2, PickingStrategyProperties), 
                               new Tuple<CreationStrategies, Dictionary<string, Property>>(strategies.Item3, CreationStrategyProperties));
            StartDiscoveryButton.IsEnabled = false;
            IntroduceAttackerButton.IsEnabled = true;
            PlotViewerButton.IsEnabled = true;
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
            var plot = app.GetPlot(PlotType.AverageVarianceLineChart);

            var propertiesEditor = new UserPropertyConfiguration(PlotType.AverageVarianceLineChart.ToString() + " Properties", plot.Properties);
            propertiesEditor.ShowDialog();

            plot = app.InitializePlot(plot, propertiesEditor.Properties);

            var plotViewer = new PlotViewer(this, plot);
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

        /*void TextBox_AllowOnlyInt(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }*/

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
        }

        private (RoutingStrategies, PickingStrategies, CreationStrategies) GetStrategies()
        {
            var routing = BaseStrategy.GetRoutingStrategiesEnum(RoutingStrategyListBox.SelectedItem.ToString());
            var picking = BaseStrategy.GetPickingStrategiesEnum(PickingStrategyListBox.SelectedItem.ToString());
            var creation = BaseStrategy.GetCreationStrategiesEnum(CreationStrategyListBox.SelectedItem.ToString());
            return (routing, picking, creation);
        }
        #endregion

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
    }
}
