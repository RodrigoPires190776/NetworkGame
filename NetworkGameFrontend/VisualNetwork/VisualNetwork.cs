using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NetworkGameFrontend.VisualNetwork
{
    public class VisualNetwork : UIElementBase
    {
        private Dictionary<Guid, VisualRouter> Routers;
        private List<VisualPacket> Packets;
        private Network.Network Network;

        public VisualNetwork(Network.Network network)
        {
            Network = network;

            Routers = new Dictionary<Guid, VisualRouter>();

            int id = 0;
            foreach(var router in network.Routers.Keys)
            {
                Routers.Add(router, new VisualRouter(id));
                id++;
            }

            Packets = new List<VisualPacket>();
        }

        public void Draw(int width, int height)
        {
            UIElement.Children.Clear();
            UIElement.Width = width;
            UIElement.Height = height;

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
                X1 = Network.Routers[link.Item1].Coordinates.X * UIElement.ActualWidth,
                X2 = Network.Routers[link.Item2].Coordinates.X * UIElement.ActualWidth,
                Y1 = Network.Routers[link.Item1].Coordinates.Y * UIElement.ActualHeight,
                Y2 = Network.Routers[link.Item2].Coordinates.Y * UIElement.ActualHeight,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2
            };

            UIElement.Children.Add(line);
        }

        private void DrawRouter(Guid routerID)
        {
            var router = Routers[routerID];          

            router.UIElement.SetValue(Canvas.LeftProperty, Network.Routers[routerID].Coordinates.X * UIElement.Width - 20);
            router.UIElement.SetValue(Canvas.TopProperty, Network.Routers[routerID].Coordinates.Y * UIElement.Height - 20);

            UIElement.Children.Add(router.UIElement);
        }
    }
}
