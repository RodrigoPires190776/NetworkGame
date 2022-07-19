using NetworkGameBackend;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;

namespace NetworkGameFrontend.VisualData.Options.Base
{
    public abstract class PieChart : BasePlot
    {
        protected List<double> Values;
        protected List<string> Labels;
        protected PiePlot PiePlot;
        public PieChart(string title, List<string> labels, Game game) :
            base(800, 600, title, game)
        {
            Labels = labels;
            Values = new List<double>();
            for (int i = 0; i < labels.Count; i++) Values.Add(0);
            PiePlot = Plot.AddPie(Values.ToArray());
            PiePlot.SliceLabels = Labels.ToArray();
        }

        protected void SetValues(List<double> values)
        {
            if (values.Count != Labels.Count) throw new Exception();
            Values = values;
            PiePlot.Values = Values.ToArray();
        }
    }
}
