using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Web.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        // To Test HomePage
        public ObservableCollection<Media> MediaCollection { get; set; }
        public Media SelectedMedia { get; set; }  
        public HomePage()
        {
            this.InitializeComponent();

            MediaCollection = new ObservableCollection<Media>();

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });
            MediaCollection.Add(new Media() { Image = "/Assets/StoreLogo.png", Name = "File1" });

            DataContext = this;
        }

        public class Media
        {
            public string Image { get; set; }
            public string Name { get; set; }
        }
    }
}
