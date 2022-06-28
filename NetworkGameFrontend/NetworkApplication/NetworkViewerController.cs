﻿using Network;
using Network.UpdateNetwork;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkGameFrontend.NetworkApplication
{
    public class NetworkViewerController
    {
        public VisualNetwork.VisualNetwork VisualNetwork { get; private set; }
        private readonly ScrollViewer NetworkScrollViewer;
        private readonly Slider Slider;
        private readonly Grid NetworkScrollViewerGrid;
        private readonly ScaleTransform NetworkScrollViewerScaleTransform;
        private readonly ContentPresenter NetworkScrollViewerContent;
        private Point? lastCenterPositionOnTarget;
        private Point? lastMousePositionOnTarget;
        private Point? lastDragPoint;

        public NetworkViewerController(ScrollViewer scrollViewer, Slider slider, Grid grid, ScaleTransform scaleTransform, ContentPresenter contentPresenter)
        {
            NetworkScrollViewer = scrollViewer;
            Slider = slider;
            NetworkScrollViewerGrid = grid;
            NetworkScrollViewerScaleTransform = scaleTransform;
            NetworkScrollViewerContent = contentPresenter;
        }

        public void LoadNetwork(string networkName)
        {
            VisualNetwork = new VisualNetwork.VisualNetwork(NetworkMaster.GetInstance().GetNetworkByName(networkName));
            VisualNetwork.Draw();
            NetworkScrollViewer.MouseLeftButtonUp += NetworkMouseReleased;
            NetworkScrollViewer.PreviewMouseLeftButtonUp += NetworkMouseReleased;
            NetworkScrollViewer.PreviewMouseLeftButtonDown += NetworkMousePressed;
            NetworkScrollViewer.MouseMove += NetworkMouseMove;
            NetworkScrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            NetworkScrollViewer.PreviewMouseWheel += ScrollMouseWheel;

            Slider.ValueChanged += OnSliderValueChanged;

            Application.Current.Dispatcher.Invoke(() => {
                var canvas = new Canvas
                {
                    Height = 2000,
                    Width = 2000
                };
                canvas.Children.Add(VisualNetwork.UIElement);
                canvas.Children.Add(VisualNetwork.PacketCanvas);
                NetworkScrollViewerContent.Content = canvas;
            });
        }

        public void Update(UpdatedState state, Guid loadedRouterID)
        {
            Application.Current.Dispatcher.Invoke(() => {
                VisualNetwork.Update(state, loadedRouterID);
            });
        }

        public void UpdateRouterData(Guid loadedRouterID)
        {
            Application.Current.Dispatcher.Invoke(() => {
                VisualNetwork.UpdateRouterData(loadedRouterID);
            });
        }

        private void NetworkMousePressed(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(NetworkScrollViewer);
            if (mousePos.X <= NetworkScrollViewer.ViewportWidth && mousePos.Y <
                NetworkScrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                NetworkScrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(NetworkScrollViewer);
            }
        }

        private void NetworkMouseReleased(object sender, MouseButtonEventArgs e)
        {
            NetworkScrollViewer.Cursor = Cursors.Arrow;
            NetworkScrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        private void NetworkMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                var currentPosition = e.GetPosition(NetworkScrollViewer);

                double dX = currentPosition.X - lastDragPoint.Value.X;
                double dY = currentPosition.Y - lastDragPoint.Value.Y;

                lastDragPoint = currentPosition;

                NetworkScrollViewer.ScrollToHorizontalOffset(NetworkScrollViewer.HorizontalOffset - dX);
                NetworkScrollViewer.ScrollToVerticalOffset(NetworkScrollViewer.VerticalOffset - dY);
            }
        }

        void ScrollMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(NetworkScrollViewer);

            if (e.Delta > 0)
            {
                Slider.Value += 0.05;
            }
            if (e.Delta < 0)
            {
                Slider.Value -= 0.05;
            }

            e.Handled = true;
        }

        void OnSliderValueChanged(object sender,
         RoutedPropertyChangedEventArgs<double> e)
        {
            NetworkScrollViewerScaleTransform.ScaleX = e.NewValue;
            NetworkScrollViewerScaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(NetworkScrollViewer.ViewportWidth / 2,
                                             NetworkScrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = NetworkScrollViewer.TranslatePoint(centerOfViewport, NetworkScrollViewerGrid);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(NetworkScrollViewer.ViewportWidth / 2,
                                                         NetworkScrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow =
                              NetworkScrollViewer.TranslatePoint(centerOfViewport, NetworkScrollViewerGrid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(NetworkScrollViewerGrid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / NetworkScrollViewerGrid.Width;
                    double multiplicatorY = e.ExtentHeight / NetworkScrollViewerGrid.Height;

                    double newOffsetX = NetworkScrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = NetworkScrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    NetworkScrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    NetworkScrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
    }
}
