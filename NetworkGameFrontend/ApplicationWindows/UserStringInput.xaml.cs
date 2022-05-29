using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace NetworkGameFrontend.ApplicationWindows
{
    public sealed partial class UserStringInput : ContentDialog
    {
        public bool Ok { get; private set; }
        public string Text { get; private set; }
        public UserStringInput(string title, string label)
        {
            InitializeComponent();
            Title = title;
            InputStringLabel.Text = label;
            Ok = false;
        }

        public UserStringInput(string title, string label, string defaultText) :
            this(title, label)
        { 
            InputStringTextBox.Text = defaultText;
        }

        private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_OkButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Text = InputStringTextBox.Text;
            Ok = true;
        }
    }
}
