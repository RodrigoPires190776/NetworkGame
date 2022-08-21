using Network.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NetworkGenerator.NetworkExporter
{
    public class NetworkExporter
    {
        public void Export(Network.Network network, string filePath)
        {
            var writer = new StreamWriter(filePath);

            writer.WriteLine(network.RouterIDList.Count);

            var routerIntIDs = new Dictionary<Guid, int>();
            var id = 0;
            foreach (var router in network.Routers.Values)
            {
                writer.WriteLine($"{GetStringFromDouble(router.Coordinates.X)},{GetStringFromDouble(router.Coordinates.Y)}");
                routerIntIDs.Add(router.ID, id++);
            }

            foreach(var router in network.Routers.Values)
            {
                foreach(var link in router.Links.Values)
                {
                    if(routerIntIDs[router.ID] < routerIntIDs[GetOtherRouter(router.ID, link)])
                    {
                        writer.WriteLine($"{routerIntIDs[router.ID]}-{routerIntIDs[GetOtherRouter(router.ID, link)]}");
                    }
                }
            }

            writer.Flush();
            writer.Close();
        }

        private Guid GetOtherRouter(Guid node, Link link)
        {
            return link.Routers.Item1 == node ? link.Routers.Item2 : link.Routers.Item1;
        }

        private string GetStringFromDouble(double value)
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }
    }
}
