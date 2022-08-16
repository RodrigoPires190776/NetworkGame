using Microsoft.Win32;
using NetworkGameFrontend.ApplicationWindows;
using NetworkGameFrontend.VisualData.Options.Base;
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
        public PlotViewer(Window owner, BasePlot plot)
        {
            Owner = owner;
            Plot = plot;
            InitializeComponent();

            PlotGrid.Children.Add(plot.WpfPlot);
        }

        void SavePlotAsImage_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = Plot.WpfPlot.Plot.Render();
            var networkNameDialog = new UserStringInput("Choose a name for the plot", "Name:", "default", this);
            _ = networkNameDialog.ShowDialog();

            if (networkNameDialog.Ok)
            {
                try
                {                   
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.FileName = networkNameDialog.Text;
                    saveDialog.DefaultExt = "jpg";
                    saveDialog.Filter = "JPG images (*.jpg)|*.jpg";

                    if (saveDialog.ShowDialog() == true)
                    { 
                        var fileName = saveDialog.FileName;
                        if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != "jpg")
                            fileName = fileName + ".jpg";

                        bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
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
