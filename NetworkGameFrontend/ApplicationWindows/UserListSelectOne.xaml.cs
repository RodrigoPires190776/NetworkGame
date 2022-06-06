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
    public sealed partial class UserListSelectOne : ContentDialog
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
            Ok = false;
        }

        private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_OkButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Ok = true;
        }
    }
}
