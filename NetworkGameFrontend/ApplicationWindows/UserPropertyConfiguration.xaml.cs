using NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkGameFrontend.ApplicationWindows
{
    /// <summary>
    /// Interaction logic for UserPropertyConfiguration.xaml
    /// </summary>
    public partial class UserPropertyConfiguration : Window
    {
        public Dictionary<string, Property>  Properties { get; private set; }
        public UserPropertyConfiguration(string title, Dictionary<string, Property> properties)
        {
            InitializeComponent();
            DialogTitle.Text = title;
            Properties = properties;

            foreach(var property in Properties)
            {
                PropertyListBox.Items.Add(new PropertyListBoxItem(property.Key, property.Value));
            }
        }

        private void Clicked_Ok(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Clicked_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class PropertyListBoxItem : ListBoxItem
    {
        private readonly string PropertyName;
        private readonly Property Property;
        private Grid Grid;
        private TextBox TextBox;

        public PropertyListBoxItem(string propertyName, Property property)
        {
            PropertyName = propertyName;
            Property = property;
            BuildItem();
            this.Content = Grid;
        }

        private void BuildItem()
        {
            Grid = new Grid();
            Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            BindingOperations.SetBinding(Grid, Grid.WidthProperty, new Binding("ActualWidth") { ElementName = "PropertyListBox" });

            TextBlock textBlock = new TextBlock
            {
                Text = PropertyName,
                FontSize = 20
            };
            _ = Grid.Children.Add(textBlock);

            TextBox = new TextBox
            {
                Text = Property.Value.ToString(),
                FontSize = 20
            };
            TextBox.SetValue(Grid.ColumnProperty, 1);
            TextBox.LostFocus += CheckValue;
            _ = Grid.Children.Add(TextBox);
        }

        private void CheckValue(object sender, RoutedEventArgs args)
        {
            var result = Property.TrySetValue(TextBox.Text);

            if(!result.Item1) MessageBox.Show($"{PropertyName}:\n{result.Item2}");
        }
    }
}
