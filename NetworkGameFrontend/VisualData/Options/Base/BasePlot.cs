using Network.UpdateNetwork;
using NetworkGameBackend;
using NetworkGameFrontend.VisualData.Options.Graphs;
using NetworkUtils;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;

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
        protected int CyclesSinceLastUpdate { get; private set; }
        protected List<UpdatedState> StatesOnHold { get; private set; }
        private int ColorIndex;
        private bool Initialized;
        public BasePlot(int width, int height, string title, Game game)
        {
            Width = width;
            Height = height;
            Plot = new Plot(width, height);
            Plot.Title(title);
            WpfPlot = new WpfPlot();
            Properties = new Dictionary<string, Property>();
            Initialized = false;
            game.GameStep += GameUpdate;
            StatesOnHold = new List<UpdatedState>();
            CyclesSinceLastUpdate = 1;
            ColorIndex = 0;
            AddBaseProperties();
        }

        private void AddBaseProperties()
        {
            Properties.Add(Property.CyclesToUpdate, new Property(Property.PropertyType.Integer,
                    new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0)
                    }));
            Properties[Property.CyclesToUpdate].SetValue(1);
            Properties.Add(Property.LoadAllValues, new Property(Property.PropertyType.Bool,
                    new List<Tuple<string, object>>()));
            Properties[Property.LoadAllValues].SetValue(true);
        }

        protected virtual void GameUpdate(object sender, UpdatedState state)
        {
            if (!Initialized) return;
            var cycles = (int)Properties[Property.CyclesToUpdate].Value;
            SaveData(state);

            if (cycles == 0 || cycles < CyclesSinceLastUpdate)
            {
                Update();
                CyclesSinceLastUpdate = 0;
                StatesOnHold.Clear();
            }
            CyclesSinceLastUpdate++;
        }

        protected virtual Color GetColor()
        {
            ColorIndex = ColorIndex + 1 < Palette.Category10.Count() ? ColorIndex + 1 : 0;
            return Palette.Category10.GetColor(ColorIndex);
        }

        public virtual BasePlot Initialize(VisualNetwork.VisualNetwork visualNetwork, Dictionary<string, Property> properties)
        {
            foreach(var property in properties)
            {
                Properties[property.Key].SetValue(property.Value.Value);
            }

            if ((bool)Properties[Property.LoadAllValues].Value) LoadPreviousData();

            Initialized = true;

            WpfPlot.Reset(Plot);
            WpfPlot.Refresh();

            return this;
        }
        protected abstract void Update();
        protected abstract void SaveData(UpdatedState state);
        protected abstract void LoadPreviousData();

        protected static Dictionary<string, Property> GetProperties(List<Tuple<string, Property.PropertyType, List<Tuple<string, object>>>> properties)
        {
            var dictionaryProperties = new Dictionary<string, Property>();

            foreach (var property in properties)
            {
                dictionaryProperties.Add(property.Item1, new Property(property.Item2, property.Item3));
            }

            dictionaryProperties.Add(Property.CyclesToUpdate, new Property(Property.PropertyType.Integer,
                   new List<Tuple<string, object>>()
                   {
                        new Tuple<string, object>(Property.INTEGER_MIN, 0)
                   }));
            dictionaryProperties[Property.CyclesToUpdate].SetValue(1);
            dictionaryProperties.Add(Property.LoadAllValues, new Property(Property.PropertyType.Bool,
                    new List<Tuple<string, object>>()));
            dictionaryProperties[Property.LoadAllValues].SetValue(true);

            return dictionaryProperties;
        }

        public enum PlotType { RouterCreatedPacketsPieChart, RouterCreatedPacketsLineChart, AverageVarianceLineChart }
        public static List<string> PlotTypeList = new List<string> { "RouterCreatedPacketsPieChart", "RouterCreatedPacketsLineChart", "AverageVarianceLineChart" };

        public static PlotType GetPlotTypeEnum(string plotType)
        {
            switch (plotType)
            {
                case "RouterCreatedPacketsPieChart":
                    return PlotType.RouterCreatedPacketsPieChart;
                case "RouterCreatedPacketsLineChart":
                    return PlotType.RouterCreatedPacketsLineChart;
                case "AverageVarianceLineChart":
                    return PlotType.AverageVarianceLineChart;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Dictionary<string, Property> GetPlotProperties(string plotType, Guid network)
        {
            switch (plotType)
            {
                case "RouterCreatedPacketsPieChart":
                    return RouterCreatedPacketsPieChart.GetProperties(network);
                case "RouterCreatedPacketsLineChart":
                    return RouterCreatedPacketsLineChart.GetProperties(network);
                case "AverageVarianceLineChart":
                    return AverageVarianceLineChart.GetProperties();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
