using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyMediaProject.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationPage : Page
    {
        public static Frame NVMain { get; private set; }
        public NavigationViewItem SelectedItem { get; set; }
        public NavigationPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            NVMain = contentFrame;
        }

        private void nvMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Settings page
            }
            else
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;
                string selectedItemTag = ((string)selectedItem.Tag);
                if (selectedItemTag.Equals("Home")) 
                {
                    contentFrame.Content = new HomePage();
                }
                else if (selectedItemTag.Equals("Play queue"))
                {
                    contentFrame.Content = new PlayQueuePage();

                }
                else if (selectedItemTag.Equals("Playlists"))
                {
                    contentFrame.Content = new PlaylistsPage();
                }
            }
        }


    }
}
