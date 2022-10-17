using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace NetworkGameFrontend.VisualData.Options.BaseCombined
{
    public abstract class LineChartCombined : BasePlot
    {
        protected List<double> Values;
        protected List<double> MaxValues;
        protected List<double> MinValues;
        protected SignalPlot SignalPlot;
        protected SignalPlot MaxValuesPlot;
        protected SignalPlot MinValuesPlot;
        private double MaxValue = 0;
        public LineChartCombined(string title, List<Game> games) :
            base(800, 600, title, games)
        {
            Values = new List<double>();
            MaxValues = new List<double>();
            MinValues = new List<double>();
            SignalPlot = new SignalPlot();
            MaxValuesPlot = new SignalPlot
            {
                Color = Color.DarkBlue
            };
            MinValuesPlot = new SignalPlot
            {
                Color = Color.DarkRed
            };

            AddValue(new List<double> { 0 });
            Plot.Add(SignalPlot);
            Plot.Add(MaxValuesPlot);
            Plot.Add(MinValuesPlot);
        }

        protected double AddValue(List<double> values)
        {
            double sum = 0;
            double localMin = double.MaxValue;
            double localMax = double.MinValue;
            foreach(var value in values)
            {
                MaxValue = Math.Max(value, MaxValue);
                localMin = Math.Min(value, localMin);
                localMax = Math.Max(value, localMax);
                sum += value;
            }
            AddMaxValue(localMax);
            AddMinValue(localMin);
            var newValue = sum / values.Count;
            Values.Add(newValue);

            Plot.SetAxisLimitsX(0, Values.Count - 1);
            Plot.SetAxisLimitsY(0, MaxValue);
            SignalPlot.Ys = Values.ToArray();
            SignalPlot.MaxRenderIndex = Values.Count - 1;

            return newValue;
        }

        private void AddMaxValue(double value)
        {
            MaxValues.Add(value);
            MaxValuesPlot.Ys = MaxValues.ToArray();
            MaxValuesPlot.MaxRenderIndex = MaxValues.Count - 1;
        }

        private void AddMinValue(double value)
        {
            MinValues.Add(value);
            MinValuesPlot.Ys = MinValues.ToArray();
            MinValuesPlot.MaxRenderIndex = MinValues.Count - 1;
        }

        public override void ResetView()
        {
            Plot.SetAxisLimitsX(0, Values.Count - 1);
            Plot.SetAxisLimitsY(0, MaxValue);
            WpfPlot.Render();
        }
    }
}
