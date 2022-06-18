using NetworkGameFrontend.ApplicationWindows;
using NetworkGameFrontend.NetworkApplication;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NetworkGameFrontend
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainApplication app;
        public MainPage()
        {
            this.InitializeComponent();
            Window.Height = 1000;
            app = new MainApplication(NetworkViewer, NetworkControls);
        }

        async void File_ImportNetwork_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".network");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            
            if (file != null)
            {
                var networkNameDialog = new UserStringInput("Choose a name for the nextwork", "Name:", file.DisplayName);
                await networkNameDialog.ShowAsync();
                if(networkNameDialog.Ok)
                {
                    try
                    {
                        var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        app.ImportNetwork(stream.AsStream(), networkNameDialog.Text);
                    }
                    catch (Exception ex)
                    {
                        var messageDialog = new MessageDialog(ex.Message, "Something went wrong!");
                        await messageDialog.ShowAsync();
                    }
                }
                else
                {
                    var messageDialog = new MessageDialog("Import canceled!");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                var messageDialog = new MessageDialog("No file selected!");
                await messageDialog.ShowAsync();
            }
        }

        async void Network_LoadNetwork_Click(object sender, RoutedEventArgs e)
        {
            var networkSelectDialog = new UserListSelectOne(app.GetAllNetworksName());
            await networkSelectDialog.ShowAsync();

            if (networkSelectDialog.Ok)
            {
                try
                {
                    app.LoadNetwork(networkSelectDialog.Item);
                }
                catch(Exception ex)
                {
                    var messageDialog = new MessageDialog(ex.Message, "Something went wrong!");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                var messageDialog = new MessageDialog("Load canceled!");
                await messageDialog.ShowAsync();
            }
        }
    
        void Controls_StartDiscovery_Click(object sender, RoutedEventArgs e)
        {
            app.StartDiscovery();
            StartDiscoveryButton.IsEnabled = false;
        }
        
        void Controls_IntroduceAttacker_Click(object sender, RoutedEventArgs e)
        {
            app.IntroduceAttacker(3, 1, 4);
        }

        void Viewer_StartPause_Click(object sender, RoutedEventArgs e)
        {
            app.ViewerStartPause();
        }

        void Viewer_SlowDown_Click(object sender, RoutedEventArgs e)
        {
            app.GameSpeedChange(-1);
        }

        void Viewer_SpeedUp_Click(object sender, RoutedEventArgs e)
        {
            app.GameSpeedChange(1);
        }

        void TextBox_AllowOnlyInt(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }
}
