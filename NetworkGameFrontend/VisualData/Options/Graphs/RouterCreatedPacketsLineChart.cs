using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameDataCollector;
using NetworkGameDataCollector.NetworkDataComponents;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs
{
    public class RouterCreatedPacketsLineChart : MultipleLineChart
    {
        private Guid Network;
        private Guid Router;
        private readonly List<RouterData> RouterDataOnHold;
        private readonly object _dataOnHoldLock = new object();
        public static Dictionary<string, Property> GetProperties(Guid network)
        {
            var properties = new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>()
            {
                new Tuple<string, Property.PropertyType, List<Tuple<string, object>>>(Property.Router, Property.PropertyType.Integer,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0),
                        new Tuple<string, object>(Property.INTEGER_MAX, NetworkMaster.GetInstance().GetNetwork(network).RouterIDList.Count - 1)
                    })
            };

            var dictionaryProperties = BasePlot.GetProperties(properties);

            dictionaryProperties[Property.Router].SetValue(0);

            return dictionaryProperties;
        }

        public RouterCreatedPacketsLineChart(Guid network, Game game) :
            base("Router Created Packets Line Chart", new List<string>() { "InTransit", "Dropped", "Delivered" }, game)
        {
            Network = network;
            Properties.Add(Property.Router, new Property(Property.PropertyType.Integer,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0),
                        new Tuple<string, object>(Property.INTEGER_MAX, NetworkMaster.GetInstance().GetNetwork(network).RouterIDList.Count - 1)
                    }));
            Properties[Property.Router].SetValue(0);
            RouterDataOnHold = new List<RouterData>();
        }

        public override BasePlot Initialize(VisualNetwork.VisualNetwork visualNetwork, Dictionary<string, Property> properties)
        {
            Router = visualNetwork.RouterIDs[(int)properties[Property.Router].Value];
            
            Plot.Legend(location: ScottPlot.Alignment.UpperLeft);
            SetTitle($"Router {(int)properties[Property.Router].Value} Created Packets Line Chart");

            base.Initialize(visualNetwork, properties);

            return this;
        }

        protected override void SaveData(UpdatedState state)
        {
            lock (_dataOnHoldLock)
            {
                RouterDataOnHold.Add(NetworkDataCollector.GetInstance().GetRouterData(Network, Router));
            }
        }

        protected override void Update()
        {
            lock (_dataOnHoldLock)
            {
                bool hasData = false;
                foreach (var routerData in RouterDataOnHold)
                {
                    SetRouterValues(routerData);
                    hasData = true;
                }
                RouterDataOnHold.Clear();
                if (hasData)
                {
                    Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        WpfPlot.Render();
                    });
                }               
            }           
        }

        private void SetRouterValues(RouterData routerData)
        {
            AddValues(new List<double> { routerData.PacketsInTransit, routerData.PacketsDropped, routerData.PacketsDelivered });
        }

        protected override void LoadPreviousData()
        {
            RouterDataOnHold.AddRange(NetworkDataCollector.GetInstance().GetPreviousRouterData(Network, Router));
        }
    }
}
