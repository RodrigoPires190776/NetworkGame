using Windows.UI.Xaml.Controls;

namespace NetworkGameFrontend.VisualNetwork
{
    public abstract class UIElementBase
    {
        public Canvas UIElement { get; private set; }
        public UIElementBase()
        {
            UIElement = new Canvas();
        }
    }
}
