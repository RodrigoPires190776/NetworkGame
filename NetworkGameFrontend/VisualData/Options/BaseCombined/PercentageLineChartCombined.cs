using NetworkGameBackend;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkGameFrontend.VisualData.Options.BaseCombined
{
    public abstract class PercentageLineChartCombined : MultipleLineChartCombined
    {
        public PercentageLineChartCombined(string title, List<string> labels, List<Game> games) :
            base(title, labels, games)
        {

        }

        protected new void AddValues(List<double> values)
        {
            var sum = 0.0;
            foreach(var value in values)
            {
                sum += value;
            }

            if(sum == 0)
            {
                base.AddValues(new List<double>(new double[values.Count]));
                return;
            }

            var normalizedValues = new List<double>();
            foreach(var value in values)
            {
                normalizedValues.Add(value / sum);
            }

            base.AddValues(normalizedValues);
        }
    }
}
