using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.BaseCombined;
using NetworkUtils;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs.Combined
{
    public class DefensorCreatedPacketsPercentageLineChartCombined : PercentageLineChartCombined
    {
        private int Dropped;
        private int Delivered;
        public static Dictionary<string, Property> GetProperties()
        {
            var dictionaryProperties = BasePlot.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>());

            return dictionaryProperties;
        }

        public DefensorCreatedPacketsPercentageLineChartCombined(List<Guid> networks, List<Game> games) :
            base("Defensor Created Packets Percentage Line Chart Combined", new List<string>() { "Dropped", "Delivered" }, games)
        {
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
            foreach (var state in states)
            {
                var packets = state.GetUpdatePackets();
                foreach (var packet in state.FinishedPackets)
                {
                    if (packets[packet].Source == NetworkMaster.GetInstance().GetNetwork(state.NetworkID).DefensorID)
                    {
                        if (packets[packet].Dropped) Dropped++;
                        else if (packets[packet].ReachedDestination) Delivered++;
                    }                   
                }
            }
        }

        protected override void Update()
        {
            AddValues(new List<double> { Dropped, Delivered });
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
        }
    }
}
