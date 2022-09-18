using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.BaseCombined;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs.Combined
{
    public class RouterCreatedPacketsLineChartCombined : MultipleLineChartCombined
    {
        private int Dropped;
        private int Delivered;
        public static Dictionary<string, Property> GetProperties()
        {
            var dictionaryProperties = BasePlot.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>());

            return dictionaryProperties;
        }

        public RouterCreatedPacketsLineChartCombined(List<Guid> networks, List<Game> games) :
            base("Router Created Packets Line Chart Combined", new List<string>() { "Dropped", "Delivered" }, games)
        {

        }

        public override BasePlot Initialize(VisualNetwork.VisualNetwork visualNetwork, Dictionary<string, Property> properties)
        {
            Plot.Legend(location: ScottPlot.Alignment.UpperLeft);

            base.Initialize(visualNetwork, properties);

            base.FinalizeInit();

            return this;
        }

        protected override void SaveData(List<UpdatedState> states)
        {
            foreach (var state in states)
            {
                var packets = state.GetUpdatePackets();
                foreach (var packet in state.FinishedPackets)
                {
                    if (packets[packet].Dropped) Dropped++;
                    else if (packets[packet].ReachedDestination) Delivered++;
                }
            }
        }

        protected override void Update()
        {
            AddValues(new List<double> { Dropped, Delivered });

            Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        WpfPlot.Render();
                    });
        }

        protected override void LoadPreviousData()
        {
            //TODO
        }
    }
}
