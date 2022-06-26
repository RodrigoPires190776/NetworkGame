using Microsoft.Win32;
using NetworkGameFrontend.ApplicationWindows;
using NetworkGameFrontend.NetworkApplication;
using NetworkGameFrontend.VisualData;
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

namespace NetworkGameFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainApplication app;
        public MainWindow()
        {
            InitializeComponent();
            app = new MainApplication(NetworkViewer, NetworkControls);
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
            app.StartDiscovery();
            StartDiscoveryButton.IsEnabled = false;
            IntroduceAttackerButton.IsEnabled = true;
            PlotViewerButton.IsEnabled = true;
        }

        void Controls_IntroduceAttacker_Click(object sender, RoutedEventArgs e)
        {
            app.IntroduceAttacker(3, 1, 4);
        }

        void Controls_PlotViewer_Click(object sender, RoutedEventArgs e)
        {
            var plot = app.GetPlot(PlotType.RouterCreatedPacketsLineChart);

            var propertiesEditor = new UserPropertyConfiguration(PlotType.RouterCreatedPacketsLineChart.ToString() + " Properties", plot.Properties);
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

        /*void TextBox_AllowOnlyInt(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }*/
    }
}
