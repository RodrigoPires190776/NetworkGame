using Network;
using Network.UpdateNetwork;
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NetworkGameFrontend.NetworkApplication
{
    public class NetworkViewerController
    {
        public VisualNetwork.VisualNetwork VisualNetwork { get; private set; }
        private ScrollViewer NetworkScrollViewer;
        private bool IsMouseDown;
        private Point MousePoint;
        private double prevX, prevY;

        public NetworkViewerController(ScrollViewer scrollViewer)
        {
            NetworkScrollViewer = scrollViewer;
        }

        public void LoadNetwork(string networkName)
        {
            VisualNetwork = new VisualNetwork.VisualNetwork(NetworkMaster.GetInstance().GetNetworkByName(networkName));
            VisualNetwork.Draw();
            NetworkScrollViewer.PointerPressed += new PointerEventHandler(NetworkMousePressed);
            NetworkScrollViewer.PointerReleased += new PointerEventHandler(NetworkMouseReleased);
            NetworkScrollViewer.PointerMoved += new PointerEventHandler(NetworkMouseMove);
            prevX = 0; prevY = 0;

            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    var canvas = new Canvas();
                    canvas.Height = 2000;
                    canvas.Width = 2000;
                    canvas.Children.Add(VisualNetwork.UIElement);
                    canvas.Children.Add(VisualNetwork.PacketCanvas);
                    NetworkScrollViewer.Content = canvas;
                });

        }

        public void Update(UpdatedState state, Guid loadedRouterID)
        {
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
               () =>
               {
                   VisualNetwork.Update(state, loadedRouterID);
               });
        }

        private void NetworkMousePressed(object sender, PointerRoutedEventArgs e)
        {
            IsMouseDown = true;
            MousePoint = e.GetCurrentPoint(NetworkScrollViewer).Position;
            NetworkScrollViewer.CapturePointer(e.Pointer);
            NetworkScrollViewer.ZoomMode = ZoomMode.Disabled;
        }

        private void NetworkMouseReleased(object sender, PointerRoutedEventArgs e)
        {
            IsMouseDown = false;
            var transform = VisualNetwork.UIElement.RenderTransform as TranslateTransform;
            if (transform != null)
            {
                prevX = transform.X;
                prevY = transform.Y;
            }
            NetworkScrollViewer.ReleasePointerCapture(e.Pointer);
            NetworkScrollViewer.ZoomMode = ZoomMode.Enabled;
        }

        private void NetworkMouseMove(object sender, PointerRoutedEventArgs e)
        {
            if (IsMouseDown)
            {
                var currentPosition = e.GetCurrentPoint(NetworkScrollViewer).Position;
                var transform = VisualNetwork.UIElement.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    VisualNetwork.UIElement.RenderTransform = transform;
                }

                transform.X = (currentPosition.X - MousePoint.X + prevX*NetworkScrollViewer.ZoomFactor) / NetworkScrollViewer.ZoomFactor;
                transform.Y = (currentPosition.Y - MousePoint.Y + prevY*NetworkScrollViewer.ZoomFactor) / NetworkScrollViewer.ZoomFactor;

                var transform2 = VisualNetwork.PacketCanvas.RenderTransform as TranslateTransform;
                if (transform2 == null)
                {
                    transform2 = new TranslateTransform();
                    VisualNetwork.PacketCanvas.RenderTransform = transform;
                }

                transform2.X = (currentPosition.X - MousePoint.X + prevX * NetworkScrollViewer.ZoomFactor) / NetworkScrollViewer.ZoomFactor;
                transform2.Y = (currentPosition.Y - MousePoint.Y + prevY * NetworkScrollViewer.ZoomFactor) / NetworkScrollViewer.ZoomFactor;
            }
        }

    }
}
