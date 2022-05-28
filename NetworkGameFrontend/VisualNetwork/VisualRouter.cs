using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NetworkGameFrontend.VisualNetwork
{
    public class VisualRouter : UIElementBase
    {
        private Ellipse Ellipse;
        private TextBlock TextBlock;
        private Dictionary<Guid, TextBlock> LinkProbabilities;
        public const int RADIUS = 20;
        public int ID { get; }
        private Guid RouterID;
        public event EventHandler<ClickedRouterEventArgs> ClickedRouter;
        public VisualRouter(int id, List<Guid> links, Guid routerID)
        {
            ID = id;
            RouterID = routerID;

            Ellipse = new Ellipse()
            {
                Width = RADIUS * 2,
                Height = RADIUS * 2,
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2
            };
            TextBlock = new TextBlock()
            {
                Text = ID.ToString(),
                FontSize = 22,
                Foreground = new SolidColorBrush(Colors.White)
            };

            UIElement.Children.Add(Ellipse);
            UIElement.Children.Add(TextBlock);

            TextBlock.SetValue(Canvas.LeftProperty, (double)14);
            TextBlock.SetValue(Canvas.TopProperty, (double)6);

            UIElement.Width = RADIUS * 2;
            UIElement.Height = RADIUS * 2;

            LinkProbabilities = new Dictionary<Guid, TextBlock>();
            foreach(var link in links)
            {
                var tBlock = CreateLinkProbability();
                LinkProbabilities.Add(link, tBlock);
                UIElement.Children.Add(tBlock);
            }

            UIElement.PointerPressed += Clicked;
        }

        private void Clicked(object sender, PointerRoutedEventArgs e)
        {
            ClickedRouter.Invoke(this, new ClickedRouterEventArgs(RouterID));
        }

        public void SetLinkProbabilityPosition(Guid id, double x1, double y1, double x2, double y2)
        {
            var tBlock = LinkProbabilities[id];
            var dirX = x2 - x1;
            var dirY = y2 - y1;

            var hipSquared = dirX * dirX + dirY * dirY;
            var hip = Math.Sqrt(hipSquared);

            dirX = dirX / hip * 30;
            dirY = dirY / hip * 30;

            tBlock.SetValue(Canvas.LeftProperty, 16 + dirX);
            tBlock.SetValue(Canvas.TopProperty, 8 + dirY);
        }

        private TextBlock CreateLinkProbability()
        {
            return new TextBlock()
            {
                Text = "...",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black)
            };
        }
    }

    public class ClickedRouterEventArgs : EventArgs
    {
        public Guid ID { get; }
        public ClickedRouterEventArgs(Guid id)
        {
            ID = id;
        }
    }
}
