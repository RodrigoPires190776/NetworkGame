using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.BaseCombined;
using NetworkUtils;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs
{
    public class AveragePacketDeliveryTimeNormalizedCombined : LineChartCombined
    {
        private List<Guid> Networks;
        private ScatterPlot ScatterPlot;
        private double[] SignalPlotXs;
        private Dictionary<Guid, List<double>> ValuesOnHold;
        private int LastX;
        private int LastUpdatedX;
        private readonly double InterpolateStep = 0.01;
        public static Dictionary<string, Property> GetProperties()
        {
            var dictionaryProperties = BasePlot.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>());

            return dictionaryProperties;
        }

        public AveragePacketDeliveryTimeNormalizedCombined(List<Guid> networks, List<Game> game) :
           base("Average Packet Delivery Time Normalized Combined", game)
        {
            Networks = networks;
            ValuesOnHold = new Dictionary<Guid, List<double>>();
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

        protected override void SaveData(List<UpdatedState> states)
        {           
            foreach(var state in states)
            {
                var value = 0.0;
                var nrValues = 0;
                foreach (var packet in state.GetUpdatePackets().Values)
                {
                    if (packet.ReachedDestination)
                    {
                        value = (double)packet.NumberOfSteps / NetworkMaster.GetInstance().GetNetwork(state.NetworkID).RouterDistances[packet.Source][packet.Destination];
                        nrValues++;
                    }
                }
                if(nrValues > 0)
                {
                    value = nrValues > 0 ? value / nrValues : 0;
                    if (ValuesOnHold.ContainsKey(state.NetworkID))
                    {
                        ValuesOnHold[state.NetworkID].Add(value);
                    }
                    else
                    {
                        ValuesOnHold.Add(state.NetworkID, new List<double>() { value });
                    }
                }              
            }
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
            var listValues = new List<double>();
            if (ValuesOnHold.Count > 0)
            {
                foreach (var list in ValuesOnHold.Values)
                {
                    var sum = 0.0;
                    foreach (var value in list)
                    {
                        sum += value;
                    }
                    listValues.Add(sum / list.Count);
                }
                
            }
            if (listValues.Count > 0) AddValue(listValues);
            else AddValue(new List<double>() { 0 });
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
