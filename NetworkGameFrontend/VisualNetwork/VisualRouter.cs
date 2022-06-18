using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NetworkGameFrontend.VisualNetwork
{
    public class VisualRouter : UIElementBase
    {
        private List<int> _offsets = new List<int>() { 0, 20, 40 };
        private int _offset = 0;
        private int Offset
        {
            get
            {
                _offset = _offset >= _offsets.Count - 1 ? 0 : _offset + 1;
                return _offsets[_offset];
            }
        }
        private Ellipse Ellipse;
        private TextBlock TextBlock;
        private Dictionary<Guid, Border> LinkProbabilities;
        public const int RADIUS = 20;
        public int ID { get; }
        private Guid RouterID;
        private RouterState State;
        public event EventHandler<ClickedRouterEventArgs> ClickedRouter;
        public VisualRouter(int id, List<Guid> links, Guid routerID)
        {
            ID = id;
            RouterID = routerID;
            State = RouterState.Normal;

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

            LinkProbabilities = new Dictionary<Guid, Border>();
            foreach(var link in links)
            {
                var tBlock = CreateLinkProbability();
                Border border = new Border()
                {
                    Background = new SolidColorBrush(Colors.White)
                };
                border.Child = tBlock;
                LinkProbabilities.Add(link, border);                
                UIElement.Children.Add(border);
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
            var offset = Offset;

            dirX = dirX / hip * (30 + offset);
            dirY = dirY / hip * (30 + offset);

            tBlock.SetValue(Canvas.LeftProperty, 9 + dirX);
            tBlock.SetValue(Canvas.TopProperty, 12 + dirY);
        }

        private TextBlock CreateLinkProbability()
        {
            return new TextBlock()
            {
                Text = "",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black)
            };
        }

        public void UpdateProbabilities(Dictionary<Guid, decimal> values)
        {
            foreach(var link in values.Keys)
            {
                ((TextBlock)LinkProbabilities[link].Child).Text = values[link].ToString("#.##");
            }
        }

        public void UpdateProbabilities()
        {
            foreach(var link in LinkProbabilities.Keys)
            {
                ((TextBlock)LinkProbabilities[link].Child).Text = "";
            }          
        }

        public void SetState(RouterState state)
        {
            switch (state)
            {
                case RouterState.Normal:
                    Ellipse.Fill = new SolidColorBrush(Colors.Blue);
                    break;
                case RouterState.Defensor:
                    Ellipse.Fill = new SolidColorBrush(Colors.Green);
                    break;
                case RouterState.Destination:
                    Ellipse.Fill = new SolidColorBrush(Colors.YellowGreen);
                    break;
                case RouterState.Attacker:
                    Ellipse.Fill = new SolidColorBrush(Colors.Red);
                    break;
            }
            State = state;
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

    public enum RouterState { Normal, Defensor, Destination, Attacker }
}
