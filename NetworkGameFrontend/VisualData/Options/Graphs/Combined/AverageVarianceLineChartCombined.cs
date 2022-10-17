using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.BaseCombined;
using NetworkUtils;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs.Combined
{
    public class AverageVarianceLineChartCombined : LineChartCombined
    {
        private List<Guid> Networks;
        private ScatterPlot ScatterPlot;
        private FunctionPlot FunctionPlot;
        private double[] SignalPlotXs;
        private int LastX;
        private int LastUpdatedX;
        private double MaxY;
        private double MinY;
        private double PercentageXValue;
        private readonly double InterpolateStep = 0.01;
        public static Dictionary<string, Property> GetProperties()
        {
            var properties = new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.Percentage, Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 1m)
                    })
            };

            var dictionaryProperties = BasePlot.GetProperties(properties);

            dictionaryProperties[Property.Percentage].SetValue(0.3m);

            return dictionaryProperties;
        }
        public AverageVarianceLineChartCombined(List<Guid> networks, List<Game> games) :
           base("Average Variance Line Chart", games)
        {
            Networks = networks;
            Properties.Add(Property.Percentage, new Property(Property.PropertyType.Decimal,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 1m)
                    }));
            Properties[Property.Percentage].SetValue(0.1m);
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

            PercentageXValue = decimal.ToDouble((decimal)properties[Property.Percentage].Value);
            FunctionPlot = new FunctionPlot(new Func<double, double?>((x) => (0)))
            {
                LineWidth = 2
            };
            FunctionPlot.Color = System.Drawing.Color.Red;
            Plot.Add(FunctionPlot);

            base.FinalizeInit();  

            return this;
        }

        protected override void LoadPreviousData()
        {
            var list = new List<List<decimal>>();

            foreach(var network in Networks)
            {
                list.Add(NetworkDataCollector.GetInstance().GetPreviousAverageVariance(network));
            }

            for(int i = 0; i < list[0].Count; i++)
            {
                var values = new List<double>();
                for(int j = 0; j < list.Count; j++) 
                {
                    values.Add(double.Parse(list[j][i].ToString()));
                }
                AddValue(values);
            }
        }

        protected override void SaveData(List<UpdatedState> states)
        {
            var list = new List<double>();
            foreach(var state in states)
            {
                if (state.UpdatedAveragevariance)
                {
                    list.Add(double.Parse(state.AverageVarience.ToString()));
                }
            }
            if(list.Count > 0)
            {
                AddValue(list);
                AddXValue(LastX + 1);
            }          
        }

        private new void AddValue(List<double> value)
        {
            UpdatePercentageLine(base.AddValue(value));
        }

        private void UpdatePercentageLine(double value)
        {
            if (value > MaxY) MaxY = value;
            if (value < MinY) MinY = value;
            if (FunctionPlot != null) FunctionPlot.Function = new Func<double, double?>((x) => MinY + ((MaxY - MinY) * PercentageXValue));
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
            if (LastUpdatedX < LastX)
            {
                var newXs = new double[SignalPlot.Ys.Length];
                Array.Copy(SignalPlotXs, 0, newXs, 0, newXs.Length);

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
