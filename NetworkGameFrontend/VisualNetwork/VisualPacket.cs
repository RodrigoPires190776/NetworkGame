using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NetworkGameFrontend.VisualNetwork
{
    public class VisualPacket
    {
        public Rectangle Rectangle { get; }
        public VisualPacket()
        {
            Rectangle =  new Rectangle()
            {
                Width = 15,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Black)
            };
        }
    }
}
