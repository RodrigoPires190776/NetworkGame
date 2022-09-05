using Network;
using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NetworkGameFrontend.VisualData.Options.Graphs
{
    public class AveragePacketDeliveryTimeNormalized : LineChart
    {
        private Guid Network;
        private List<double> ValuesOnHold;
        public static Dictionary<string, Property> GetProperties()
        {
            var dictionaryProperties = BasePlot.GetProperties(new List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>>());

            return dictionaryProperties;
        }
        public AveragePacketDeliveryTimeNormalized(Guid network, Game game) :
           base("Average Packet Delivery Time Normalized", game)
        {
            Network = network;
            ValuesOnHold = new List<double>();
        }

        public override BasePlot Initialize(VisualNetwork.VisualNetwork visualNetwork, Dictionary<string, Property> properties)
        {
            base.Initialize(visualNetwork, properties);

            base.FinalizeInit();

            return this;
        }

        protected override void LoadPreviousData()
        {
            //TODO
        }

        protected override void SaveData(UpdatedState state)
        {
            var value = 0.0;
            var nrValues = 0;
            foreach(var packet in state.GetUpdatePackets().Values)
            {
                if (packet.ReachedDestination)
                {
                    var network = NetworkMaster.GetInstance().GetNetwork(Network);
                    value = (double)packet.NumberOfSteps / NetworkMaster.GetInstance().GetNetwork(Network).RouterDistances[packet.Source][packet.Destination];
                    nrValues++;
                }
            }
            if (nrValues > 0) ValuesOnHold.Add(value / nrValues);
        }

        protected override void Update()
        {
            if (ValuesOnHold.Count > 0)
            {
                var sum = 0.0;
                foreach(var value in ValuesOnHold)
                {
                    sum += value;
                }
                AddValue(sum / ValuesOnHold.Count);
            }
            else AddValue(0);
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    WpfPlot.Render();
                });
        }
    }
}
