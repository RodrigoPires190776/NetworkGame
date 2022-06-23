using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkUtils;
using ScottPlot;
using System;
using System.Collections.Generic;

namespace NetworkGameFrontend.VisualData.Options.Base
{
    public abstract class BasePlot
    {
        protected int Width { get; set; }
        protected int Height { get; set; }
        protected string Title { get; set; }
        protected Plot Plot { get; private set; }
        public WpfPlot WpfPlot { get; private set; }
        public Dictionary<string, Property> Properties { get; }
        public BasePlot(int width, int height, string title, Game game)
        {
            Width = width;
            Height = height;
            Plot = new Plot(width, height);
            Plot.Title(title);
            WpfPlot = new WpfPlot();
            Properties = new Dictionary<string, Property>();
            game.GameStep += Update;
        }

        public abstract BasePlot Initialize(VisualNetwork.VisualNetwork visualNetwork);
        public abstract void Update(object sender, UpdatedState state);
    }
}
