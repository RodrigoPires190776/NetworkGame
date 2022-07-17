using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs
{
    public class AverageVarianceLineChart : LineChart
    {
        private Guid Network;
        private ScatterPlot ScatterPlot;
        private double[] SignalPlotXs;
        private int LastX;
        private int LastUpdatedX;
        private double InterpolateStep = 0.01;
        public static Dictionary<string, Property> GetProperties()
        {
            var properties = new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>();

            var dictionaryProperties = BasePlot.GetProperties(properties);

            return dictionaryProperties;
        }
        public AverageVarianceLineChart(Guid network, Game game) :
           base("Average Variance Line Chart", game)
        {
            Network = network;
        }

        public override BasePlot Initialize(VisualNetwork.VisualNetwork visualNetwork, Dictionary<string, Property> properties)
        {
            base.Initialize(visualNetwork, properties);

            SignalPlotXs = new double[Values.Count];

            for (int x = 0; x < Values.Count; x++) SignalPlotXs[x] = x;
            LastX = Values.Count - 1;
            LastUpdatedX = LastX;

            (double[] bzX, double[] bzY) = ScottPlot.Statistics.Interpolation.Bezier.InterpolateXY(SignalPlotXs, SignalPlot.Ys, InterpolateStep);

            ScatterPlot = new ScatterPlot(bzX, bzY)
            {
                LineWidth = 2
            };
            Plot.Add(ScatterPlot);

            return this;
        }

        protected override void LoadPreviousData()
        {
            var averageVariances = NetworkDataCollector.GetInstance().GetPreviousAverageVariance(Network);

            foreach(var value in averageVariances)
            {
                AddValue(double.Parse(value.ToString()));
            }
        }

        protected override void SaveData(UpdatedState state)
        {
            if (state.UpdatedAveragevariance)
            {
                AddValue(double.Parse(state.AverageVarience.ToString()));
                AddXValue(LastX + 1);
            }
        }

        private void AddXValue(double value)
        {
            if(SignalPlotXs.Length <= LastX + 1)
            {
                Array.Resize(ref SignalPlotXs, SignalPlotXs.Length * 2);
            }
            SignalPlotXs[++LastX] = value;
        }

        protected override void Update()
        {
            if(LastUpdatedX < LastX)
            {
                var newXs = new double[SignalPlot.Ys.Length];
                Array.Copy(SignalPlotXs, 0, newXs, 0, newXs.Length);

                //var newYs = new double[LastX - LastUpdatedX];
                //Array.Copy(SignalPlot.Ys, LastX, newYs, 0, LastX - LastUpdatedX);

                (double[] bzX, double[] bzY) = ScottPlot.Statistics.Interpolation.Bezier.InterpolateXY(newXs, SignalPlot.Ys, InterpolateStep);

                ScatterPlot.Update(bzX, bzY);
                LastUpdatedX = LastX;
            }
            
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    WpfPlot.Render();
                });
        }
    }
}
