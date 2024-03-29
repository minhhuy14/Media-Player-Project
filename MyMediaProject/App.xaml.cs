﻿using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyMediaProject.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyMediaProject
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow() { Content = new NavigationPage() };

            m_window.Title = "Infinity Media Player";

            // resize
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon(@"Assets/app_icon.ico");
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1600, Height = 900 });

            // move to center screen
            PointInt32 CenteredPosition = appWindow.Position;
            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
            CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
            CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
            appWindow.Move(CenteredPosition);

            m_window.ExtendsContentIntoTitleBar = false;
            m_window.SetTitleBar(null);
            m_window.Activate();
            m_window.Closed += async (sender, e) =>
            {
                try
                {
                    if (MainRoot != null)
                    {
                        e.Handled = true;
                        var _dataServices = new DataServices();
                        await _dataServices.ClearPlayQueue();
                        ((NavigationPage)m_window.Content)?.Dispose();
                        m_window.Content = null;
                        MainRoot = null;
                        Current.Exit();
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            };

            MainRoot = m_window.Content as FrameworkElement;
        }

        private Window m_window;
        public static FrameworkElement MainRoot { get; private set; }
    }
}
