using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaProject.Pages;
using System.Reflection.Metadata.Ecma335;

namespace MyMediaProject.Helpers
{
    public static class DialogService
    {
        public static async Task<string> ShowCreateDialogDialog(
            this FrameworkElement element,
            string title,
            string yesButtonText,
            string cancelButtonText)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = title,
                PrimaryButtonText = yesButtonText,
                CloseButtonText = cancelButtonText,
                DefaultButton = ContentDialogButton.Close,
                Content = new CreatePlaylistDialogPage(),
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                XamlRoot = element.XamlRoot,
                RequestedTheme = element.ActualTheme,
            };

            var flag = await contentDialog.ShowAsync();

            if (flag == ContentDialogResult.None)
            {
                return null;
            }
            else 
            {
                return ((CreatePlaylistDialogPage)contentDialog.Content).Playlist;
            }
        }
    }
}
