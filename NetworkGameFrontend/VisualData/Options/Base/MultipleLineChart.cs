using NetworkGameBackend;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NetworkGameFrontend.VisualData.Options.Base
{
    public abstract class MultipleLineChart : BasePlot
    {
        protected List<List<double>> Values;
        protected List<SignalPlot> SignalPlots;
        private double MaxValue = 0;
        public MultipleLineChart(string title, List<string> labels, Game game) :
            base(800, 600, title, game)
        {
            Values = new List<List<double>>();
            for (int i = 0; i < labels.Count; i++) Values.Add(new List<double>());
            SignalPlots = new List<SignalPlot>();

            for (int i = 0; i < labels.Count; i++)
            {
                SignalPlots.Add(new SignalPlot());
                SignalPlots[i].Label = labels[i];
                SignalPlots[i].LineColor = GetColor();
                SignalPlots[i].FillBelow(SignalPlots[i].LineColor, alpha: 0.5);
            }
            var initList = new List<double>();
            foreach (var _ in SignalPlots) initList.Add(0);
            AddValues(initList);
            for (int i = labels.Count - 1; i >= 0; i--) Plot.Add(SignalPlots[i]);
        }

        protected void AddValues(List<double> values)
        {
            double sum = 0;
            
            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
                Values[i].Add(sum);                

                SignalPlots[i].Ys = Values[i].ToArray();
                SignalPlots[i].MaxRenderIndex = Values[0].Count - 1;               
            }

            MaxValue = Math.Max(sum, MaxValue);
            Plot.SetAxisLimitsX(0, Values[0].Count);
            Plot.SetAxisLimitsY(0, MaxValue);
        }
    }
}

