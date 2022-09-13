using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameDataCollector.NetworkDataComponents;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.BaseCombined;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs
{
    public class RouterCreatedPacketsPercentageLineChartCombined : PercentageLineChartCombined
    {
        //private int InTransit;
        private int Dropped;
        private int Delivered;
        public static Dictionary<string, Property> GetProperties()
        {
            var dictionaryProperties = BasePlot.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>());

            return dictionaryProperties;
        }

        public RouterCreatedPacketsPercentageLineChartCombined(List<Guid> networks, List<Game> games) :
            base("Router Created Packets Percentage Line Chart Combined", new List<string>() { /*"InTransit",*/ "Dropped", "Delivered" }, games)
        {
            //InTransit = 0;
            Dropped = 0;
            Delivered = 0;
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
            foreach(var state in states)
            {
                var packets = state.GetUpdatePackets();
                foreach(var packet in state.FinishedPackets)
                {
                    if (packets[packet].Dropped) Dropped++;
                    else if (packets[packet].ReachedDestination) Delivered++;
                }
                //InTransit += packets.Count - state.FinishedPackets.Count;
            }
        }

        protected override void Update()
        {
            AddValues(new List<double> {/* InTransit, */Dropped, Delivered });
            //InTransit = 0;
            Dropped = 0;
            Delivered = 0;
            Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        WpfPlot.Render();
                    });                         
        }

        protected override void LoadPreviousData()
        {
            //TODO
            //RouterDataOnHold.AddRange(NetworkDataCollector.GetInstance().GetPreviousRouterData(Network, Router));
        }
    }
}
