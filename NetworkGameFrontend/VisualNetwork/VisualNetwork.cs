﻿using Network.UpdateNetwork;
using Network.UpdateNetwork.UpdateObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NetworkGameFrontend.VisualNetwork
{
    public class VisualNetwork : UIElementBase
    {
        public readonly Canvas PacketCanvas;
        private UpdatedState LastState;
        public readonly Dictionary<Guid, VisualRouter> Routers;
        private readonly Dictionary<Guid, List<Coordinates>> LinkPositions;
        private readonly Network.Network Network;
        public const int WIDTH = 2000; 
        public const int HEIGHT = 2000;

        public VisualNetwork(Network.Network network)
        {
            Network = network;
            PacketCanvas = new Canvas();
            PacketCanvas.Height = HEIGHT;
            PacketCanvas.Width = WIDTH;

            Routers = new Dictionary<Guid, VisualRouter>();

            int id = 0;
            foreach(var router in network.Routers.Keys)
            {
                List<Guid> links = network.Routers[router].Links.Keys.ToList();
                Routers.Add(router, new VisualRouter(id, links, router));
                id++;
            }

            LinkPositions = new Dictionary<Guid, List<Coordinates>>();

            foreach(var link in network.Links)
            {
                var x1 = network.Routers[link.Routers.Item1].Coordinates.X * WIDTH + VisualRouter.RADIUS - (double)VisualPacket.WIDTH/2;
                var y1 = network.Routers[link.Routers.Item1].Coordinates.Y * HEIGHT + VisualRouter.RADIUS - (double)VisualPacket.HEIGHT/2;
                var x2 = network.Routers[link.Routers.Item2].Coordinates.X * WIDTH + VisualRouter.RADIUS - (double)VisualPacket.WIDTH/2;
                var y2 = network.Routers[link.Routers.Item2].Coordinates.Y * HEIGHT + VisualRouter.RADIUS - (double)VisualPacket.HEIGHT/2;
                var xStep = (x2 - x1) / link.LinkLength;
                var yStep = (y2 - y1) / link.LinkLength;
                var list = new List<Coordinates>();

                for (int i = 0; i < link.LinkLength; i++){
                    list.Add(new Coordinates(x1 + xStep * i, y1 + yStep * i));
                }

                LinkPositions.Add(link.ID, list);
                Routers[link.Routers.Item1].SetLinkProbabilityPosition(link.ID, x1, y1, x2, y2);
                Routers[link.Routers.Item2].SetLinkProbabilityPosition(link.ID, x2, y2, x1, y1);
            }
        }

        public void Update(UpdatedState state, Guid loadedRouterID)
        {
            PacketCanvas.Children.Clear();
            LastState = state;

            foreach (var link in state.UpdatedLinks.Values)
            {
                foreach (var packet in link.PackagesInTransit)
                {
                    var vPacket = new VisualPacket(packet.Value.NrSteps);
                    var coordinates = LinkPositions[link.ID][packet.Value.PositionInLink];
                    vPacket.UIElement.SetValue(Canvas.LeftProperty, coordinates.X);
                    vPacket.UIElement.SetValue(Canvas.TopProperty, coordinates.Y);
                    PacketCanvas.Children.Add(vPacket.UIElement);
                }
            }
            
            UpdateRouterData(loadedRouterID);           
        }

        public void UpdateRouterData(Guid loadedRouterID)
        {
            if (loadedRouterID != Guid.Empty)
            {
                foreach (var router in Network.RouterIDList)
                {
                    if (router != loadedRouterID) Routers[router].UpdateProbabilities(LastState.UpdatedRouters[router].RoutingTable.GetPercentageValues(loadedRouterID));
                    else Routers[router].UpdateProbabilities();
                }
            }
        }

        public void Draw()
        {
            UIElement.Background = new SolidColorBrush(Colors.Transparent);
            UIElement.Width = WIDTH;
            UIElement.Height = HEIGHT;

            foreach (var link in Network.Links)
            {
                DrawLink(link.Routers);
            }

            foreach (var router in Network.Routers.Keys)
            {
                DrawRouter(router);
            }
        } 

        private void DrawLink(Tuple<Guid,Guid> link)
        {
            Line line = new Line()
            {
                X1 = Network.Routers[link.Item1].Coordinates.X * UIElement.ActualWidth + VisualRouter.RADIUS,
                X2 = Network.Routers[link.Item2].Coordinates.X * UIElement.ActualWidth + VisualRouter.RADIUS,
                Y1 = Network.Routers[link.Item1].Coordinates.Y * UIElement.ActualHeight + VisualRouter.RADIUS,
                Y2 = Network.Routers[link.Item2].Coordinates.Y * UIElement.ActualHeight + VisualRouter.RADIUS,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2
            };

            UIElement.Children.Add(line);
        }

        private void DrawRouter(Guid routerID)
        {
            var router = Routers[routerID];          

            router.UIElement.SetValue(Canvas.LeftProperty, Network.Routers[routerID].Coordinates.X * UIElement.Width);
            router.UIElement.SetValue(Canvas.TopProperty, Network.Routers[routerID].Coordinates.Y * UIElement.Height);

            UIElement.Children.Add(router.UIElement);
        }
 
    }

    public class Coordinates
    {
        public double X { get; }
        public double Y { get; }

        public Coordinates(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
