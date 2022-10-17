using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs.Single
{
    public class AveragePacketDeliveryTimeNormalized : LineChart
    {
        private Guid Network;
        private ScatterPlot ScatterPlot;
        private double[] SignalPlotXs;
        private List<double> ValuesOnHold;
        private int LastX;
        private int LastUpdatedX;
        private readonly double InterpolateStep = 0.01;
        public static Dictionary<string, Property> GetProperties()
        {
            var dictionaryProperties = BasePlot.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>());

            return dictionaryProperties;
        }
        public AveragePacketDeliveryTimeNormalized(Guid network, Game game) :
           base("Average Packet Delivery Time Normalized", game)
        {
            Network = network;
            ValuesOnHold = new List<double>();
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

            base.FinalizeInit();

            return this;
        }

        protected override void LoadPreviousData()
        {
            //TODO
        }

        protected override void SaveData(UpdatedState state)
        {
            var value = 0.0;
            var nrValues = 0;
            foreach(var packet in state.GetUpdatePackets().Values)
            {
                if (packet.ReachedDestination)
                {
                    value = (double)packet.NumberOfSteps / NetworkMaster.GetInstance().GetNetwork(Network).RouterDistances[packet.Source][packet.Destination];
                    nrValues++;
                }
            }
            if (nrValues > 0) ValuesOnHold.Add(value / nrValues);
        }
        private void AddXValue(double value)
        {
            if (SignalPlotXs.Length <= LastX + 1)
            {
                Array.Resize(ref SignalPlotXs, SignalPlotXs.Length * 2);
            }
            SignalPlotXs[++LastX] = value;
        }

        protected override void Update()
        {
            var newValue = 0.0;
            if (ValuesOnHold.Count > 0)
            {
                var sum = 0.0;
                foreach(var value in ValuesOnHold)
                {
                    sum += value;
                }
                newValue = sum / ValuesOnHold.Count;
            }
            AddValue(newValue);
            AddXValue(LastX + 1);
            if (LastUpdatedX < LastX)
            {
                var newXs = new double[SignalPlot.Ys.Length];
                Array.Copy(SignalPlotXs, 0, newXs, 0, newXs.Length);

                (double[] bzX, double[] bzY) = ScottPlot.Statistics.Interpolation.Bezier.InterpolateXY(newXs, SignalPlot.Ys, InterpolateStep);

                ScatterPlot.Update(bzX, bzY);
                LastUpdatedX = LastX;
            }
            ValuesOnHold.Clear();
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    WpfPlot.Render();
                });
        }
    }
}
