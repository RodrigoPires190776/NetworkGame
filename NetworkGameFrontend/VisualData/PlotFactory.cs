using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.Graphs;
using System;

namespace NetworkGameFrontend.VisualData
{
    public static class PlotFactory
    {
        public static BasePlot GetPlot(PlotType type, Guid network, Game game)
        {
            return type switch
            {
                PlotType.RouterCreatedPackets => new RouterCreatedPacketsPieChart(network, game),
                _ => throw new NotImplementedException()
            };
        }
    }

    public enum PlotType { RouterCreatedPackets }
}
