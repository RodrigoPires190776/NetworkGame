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
    /// Interaction logic for UserStringInput.xaml
    /// </summary>
    public partial class UserStringInput : Window
    {
        public bool Ok { get; private set; }
        public string Text { get; private set; }
        public UserStringInput(string title, string label, Window owner)
        {
            Owner = owner;
            InitializeComponent();
            DialogTitle.Text = title;
            InputStringLabel.Text = label;
            Ok = false;
        }

        public UserStringInput(string title, string label, string defaultText, Window owner) :
            this(title, label, owner)
        {
            InputStringTextBox.Text = defaultText;
        }

        private void Clicked_Ok(object sender, RoutedEventArgs e)
        {
            Text = InputStringTextBox.Text;
            Ok = true;
            Close();
        }

        private void Clicked_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
