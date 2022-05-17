using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NetworkGameFrontend.VisualNetwork
{
    public class VisualRouter : UIElementBase
    {
        private Ellipse Ellipse;
        private TextBlock TextBlock;
        private int ID;
        public VisualRouter(int id)
        {
            ID = id;

            Ellipse = new Ellipse()
            {
                Width = 40,
                Height = 40,
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

            UIElement.Width = 40;
            UIElement.Height = 40;      
        }
    }
}
