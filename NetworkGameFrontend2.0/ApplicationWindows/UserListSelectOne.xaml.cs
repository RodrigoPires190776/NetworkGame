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
    /// Interaction logic for UserListSelectOne.xaml
    /// </summary>
    public partial class UserListSelectOne : Window
    {
        private const string DefaultValue = "...";
        private bool _ok;
        public bool Ok
        {
            get
            {
                return SelectedItem != DefaultValue && _ok;
            }
            set
            {
                _ok = value;
            }
        }
        public string Item { get { return SelectedItem; } }
        private readonly List<string> Itens;
        private string SelectedItem;
        public UserListSelectOne(List<string> itens)
        {
            InitializeComponent();
            Itens = new List<string>() { DefaultValue };
            Itens.AddRange(itens);
            SelectedItem = Itens[0];
            ListBox.ItemsSource = Itens;
            ListBox.SelectedItem = SelectedItem;
            Ok = false;
        }

        private void Clicked_Ok(object sender, RoutedEventArgs e)
        {
            Ok = true;
            SelectedItem = (string)ListBox.SelectedItem;
            Close();
        }

        private void Clicked_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
