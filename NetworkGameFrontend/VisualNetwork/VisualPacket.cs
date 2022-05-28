using Network;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NetworkGameFrontend.VisualNetwork
{
    public class VisualPacket : UIElementBase
    {
        private Rectangle Rectangle;
        public const int WIDTH = 15;
        public const int HEIGHT = 10;
        public VisualPacket(int nrSteps)
        {
            Rectangle = new Rectangle()
            {
                Width = WIDTH,
                Height = HEIGHT,
                Fill = new SolidColorBrush(GetColor(nrSteps))
            };

            UIElement.Children.Add(Rectangle);
        }

        private Color GetColor(int nrSteps)
        {
            if(nrSteps < NetworkMaster.PacketTTL / 4)
            {
                return Colors.Black;
            }
            if (nrSteps < NetworkMaster.PacketTTL / 2)
            {
                return Colors.Yellow;
            }
            if (nrSteps < 3 * NetworkMaster.PacketTTL / 4)
            {
                return Colors.Orange;
            }
            else
            {
                return Colors.Red;
            }
        }
    }
}
