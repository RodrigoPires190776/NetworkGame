using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.Graphs;
using System;
using static NetworkGameFrontend.VisualData.Options.Base.BasePlot;

namespace NetworkGameFrontend.VisualData
{
    public static class PlotFactory
    {
        public static BasePlot GetPlot(PlotType type, Guid network, Game game)
        {
            return type switch
            {
                PlotType.RouterCreatedPacketsPieChart => new RouterCreatedPacketsPieChart(network, game),
                PlotType.RouterCreatedPacketsLineChart => new RouterCreatedPacketsLineChart(network, game),
                PlotType.AverageVarianceLineChart => new AverageVarianceLineChart(network, game),
                _ => throw new NotImplementedException()
            };
        }
    }
}
