using NetworkGameFrontend.VisualData.Options.Base;
using ScottPlot;
using System;
using System.Collections.Generic;
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
    }
}
