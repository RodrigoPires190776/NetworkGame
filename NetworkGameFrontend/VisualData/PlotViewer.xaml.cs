using Microsoft.Win32;
using NetworkGameFrontend.ApplicationWindows;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkGameFrontend.VisualData
{
    /// <summary>
    /// Interaction logic for PlotViewer.xaml
    /// </summary>
    public partial class PlotViewer : Window
    {
        private BasePlot Plot;
        //private int NetworkID;
        public PlotViewer(Window owner, BasePlot plot, int networkID, bool allGames = false)
        {
            Owner = owner;
            Plot = plot;
            InitializeComponent();

            PlotGrid.Children.Add(plot.WpfPlot);
            NetworkIDTextBlock.Text = allGames ? "ALL" : networkID.ToString();
            CyclesTextBlock.Text = ((int)plot.Properties[Property.CyclesToUpdate].Value).ToString();
        }

        void ResetView_Click(object sender, RoutedEventArgs e)
        {
            Plot.ResetView();
        }

        void SavePlotAsImage_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = new Bitmap(2400,1600);
            Plot.WpfPlot.Plot.Render(bmp, false, 4);
            var networkNameDialog = new UserStringInput("Choose a name for the plot", "Name:", Plot.Title, this);
            _ = networkNameDialog.ShowDialog();

            if (networkNameDialog.Ok)
            {
                try
                {                   
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.FileName = networkNameDialog.Text;
                    saveDialog.DefaultExt = "Png";
                    saveDialog.Filter = "Png images (*.Png)|*.Png";

                    if (saveDialog.ShowDialog() == true)
                    { 
                        var fileName = saveDialog.FileName;

                        bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
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
    }
}
