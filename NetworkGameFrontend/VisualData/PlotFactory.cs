using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Base;
using NetworkGameFrontend.VisualData.Options.Graphs;
using System;
using System.Collections.Generic;
using static NetworkGameFrontend.VisualData.Options.Base.BasePlot;

namespace NetworkGameFrontend.VisualData
{
    public static class PlotFactory
    {
        public static BasePlot GetPlot(PlotType type, Guid network, Game game, List<Guid> networks, List<Game> games)
        {
            return type switch
            {
                PlotType.RouterCreatedPacketsPieChart => new RouterCreatedPacketsPieChart(network, game),
                PlotType.RouterCreatedPacketsLineChart => new RouterCreatedPacketsLineChart(network, game),
                PlotType.AverageVarianceLineChart => new AverageVarianceLineChart(network, game),
                PlotType.AverageVarianceLineChartCombined => new AverageVarianceLineChartCombined(networks, games),
                PlotType.RouterCreatedPacketsPercentageLineChartCombined => new RouterCreatedPacketsPercentageLineChartCombined(networks, games),
                PlotType.RouterCreatedPacketsLineChartCombined => new RouterCreatedPacketsLineChartCombined(networks, games),
                PlotType.AveragePacketDeliveryTimeNormalized => new AveragePacketDeliveryTimeNormalized(network, game),
                PlotType.AveragePacketDeliveryTimeNormalizedCombined => new AveragePacketDeliveryTimeNormalizedCombined(networks, games),
                _ => throw new NotImplementedException()
            };
        }
    }
}
