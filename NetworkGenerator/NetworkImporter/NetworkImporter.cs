using Network.Components;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NetworkGenerator.NetworkImporter
{
    public abstract class NetworkImporter<T>
    {
        protected int SmallestLink { get; set; }
        protected abstract void Initialize(T networkData, int smallestLink);
        protected abstract string GetNextLine();
        public abstract Network.Network Import(T networkData, int smallestLink);

        private enum ImportState { STARTED, COORDINATES, LINKS }
        protected virtual Network.Network Import()
        {
            Network.Network network = new Network.Network();

            int nrRouters = 0;
            int currentRouter = 0;
            List<TempLink> tempLinks = new List<TempLink>();
            ImportState state = ImportState.STARTED;
            string line = GetNextLine();

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
                line = GetNextLine();
            }

            double minDist = double.MaxValue;
            foreach (TempLink link in tempLinks)
            {
                var r1 = network.Routers[link.Routers.Item1];
                var r2 = network.Routers[link.Routers.Item2];
                link.Distance = Distance(r1, r2);
                if (link.Distance < minDist) minDist = link.Distance;
            }

            double minDistUnit = minDist / SmallestLink;

            foreach (TempLink link in tempLinks)
            {
                var realDistance = (int)Math.Ceiling(link.Distance / minDistUnit);
                var networkLink = new Link(link.Routers.Item1, link.Routers.Item2, realDistance, network.ID);
                network.AddLink(networkLink);
            }

            int id = 0;
            foreach (var router in network.Routers.Values)
            {
                if (router.Links.Count == 0) throw new Exception($"Router {id} with has no links!");
                id++;
            }

            return network;
        }

        protected static float Distance(Router r1, Router r2)
        {
            var xSquared = r2.Coordinates.X - r1.Coordinates.X;
            xSquared *= xSquared;
            var ySquared = r2.Coordinates.Y - r1.Coordinates.Y;
            ySquared *= ySquared;
            return (float)Math.Sqrt(xSquared + ySquared);
        }
        protected class TempLink
        {
            internal Tuple<Guid, Guid> Routers { get; }
            internal double Distance { get; set; }

            internal TempLink(Guid r1, Guid r2)
            {
                Routers = new Tuple<Guid, Guid>(r1, r2);
            }
        }
    }
}
