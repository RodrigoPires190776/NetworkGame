﻿using NetworkGameBackend;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;

namespace NetworkGameFrontend.VisualData.Options.Base
{
    public abstract class LineChart : BasePlot
    {
        protected List<double> Values;
        protected SignalPlot SignalPlot;
        private double MaxValue = 1;
        public LineChart(string title, Game game) :
            base(800, 600, title, game)
        {
            Values = new List<double>();
            SignalPlot = new SignalPlot();
            AddValue(0);
            Plot.Add(SignalPlot);
        }

        protected void AddValue(double value)
        {
            MaxValue = Math.Max(value, MaxValue);
            Values.Add(value);
            
            Plot.SetAxisLimitsX(0, Values.Count);
            Plot.SetAxisLimitsY(0, MaxValue);
            SignalPlot.Ys = Values.ToArray();
            SignalPlot.MaxRenderIndex = Values.Count - 1;
        }
    }
}