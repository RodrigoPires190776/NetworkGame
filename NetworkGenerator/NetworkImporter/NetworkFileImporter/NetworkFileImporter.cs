using Network.Components;
using Network.Strategies;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NetworkGenerator.NetworkImporter.NetworkFile
{
    public class NetworkFileImporter : INetworkImporter<Stream>
    {
        private enum ImportState { STARTED, COORDINATES, LINKS }
        public Network.Network Import(Stream fileStream, int smallestLink)
        {
            Network.Network network = new Network.Network();

            StreamReader reader = new StreamReader(fileStream);
            int nrRouters = 0;
            int currentRouter = 0;
            List<TempLink> tempLinks = new List<TempLink>();
            ImportState state = ImportState.STARTED;
            string line = reader.ReadLine();

            while (line != null)
            {
                if (state == ImportState.STARTED)
                {
                    nrRouters = Int32.Parse(line);
                    state = ImportState.COORDINATES;
                }
                else if (state == ImportState.COORDINATES)
                {
                    var coordinates = line.Split(',');
                    var router = new Router(network.ID, new Coordinates(
                        double.Parse(coordinates[0], CultureInfo.InvariantCulture), 
                        double.Parse(coordinates[1], CultureInfo.InvariantCulture)));
                    network.AddRouter(router);

                    currentRouter++;
                    if (currentRouter >= nrRouters) state = ImportState.LINKS;
                }
                else if (state == ImportState.LINKS)
                {
                    var routers = line.Split('-');
                    var id1 = network.RouterIDList[Int32.Parse(routers[0])];
                    var id2 = network.RouterIDList[Int32.Parse(routers[1])];

                    tempLinks.Add(new TempLink(id1, id2));
                }
                line = reader.ReadLine();               
            }

            double minDist = double.MaxValue;
            foreach (TempLink link in tempLinks)
            {
                var r1 = network.Routers[link.Routers.Item1];
                var r2 = network.Routers[link.Routers.Item2];
                link.Distance = Distance(r1, r2);
                if (link.Distance < minDist) minDist = link.Distance;
            }

            double minDistUnit = minDist / smallestLink;

            foreach (TempLink link in tempLinks)
            {
                var realDistance = (int)Math.Ceiling(link.Distance / minDistUnit);
                var networkLink = new Link(link.Routers.Item1, link.Routers.Item2, realDistance, network.ID);
                network.AddLink(networkLink);
            }

            return network;
        }
        private static float Distance(Router r1, Router r2)
        {
            var xSquared = r2.Coordinates.X - r1.Coordinates.X;
            xSquared *= xSquared;
            var ySquared = r2.Coordinates.Y - r1.Coordinates.Y;
            ySquared *= ySquared;
            return (float)Math.Sqrt(xSquared + ySquared);
        }
    }

    internal class TempLink
    {
        internal Tuple<Guid,Guid> Routers { get; }
        internal double Distance { get; set; }

        internal TempLink(Guid r1, Guid r2)
        {
            Routers = new Tuple<Guid,Guid>(r1, r2);
        }
    }
}
