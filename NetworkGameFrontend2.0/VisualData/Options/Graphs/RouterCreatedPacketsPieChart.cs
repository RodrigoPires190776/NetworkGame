using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameDataCollector.NetworkDataComponents;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs
{
    public class RouterCreatedPacketsPieChart : PieChart
    {
        private Guid Network;
        private Guid Router;

        public RouterCreatedPacketsPieChart(Guid network, Game game) :
            base("Router Created Packets Pie Chart", new List<string>() { "Delivered", "InTransit", "Dropped" }, game)
        {
            Network = network;
            Properties.Add("Router", new Property(Property.PropertyType.Integer, 
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0),
                        new Tuple<string, object>(Property.INTEGER_MAX, NetworkMaster.GetInstance().GetNetwork(network).RouterIDList.Count - 1)
                    }));
        }

        public override BasePlot Initialize(VisualNetwork.VisualNetwork visualNetwork)
        {
            Router = visualNetwork.RouterIDs[(int)Properties["Router"].Value];
            SetRouterValues(NetworkDataCollector.GetInstance().GetRouterData(Network, Router));
            
            PiePlot.ShowPercentages = true;
            PiePlot.ShowValues = true;
            PiePlot.ShowLabels = true;
            Plot.Legend();

            WpfPlot.Reset(Plot);
            WpfPlot.Refresh();

            return this;
        }

        public override void Update(object sender, UpdatedState state)
        {
            SetRouterValues(NetworkDataCollector.GetInstance().GetRouterData(Network, Router));
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    WpfPlot.Refresh();
                });
        }

        private void SetRouterValues(RouterData routerData)
        {
            SetValues(new List<double> { routerData.PacketsDelivered, routerData.PacketsInTransit, routerData.PacketsDropped });
        }
    }
}
