﻿using ColorAndDepth.ViewModels;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ColorAndDepth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel _vm;
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _vm.Kinect.Stop();
        }
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = new MainPageViewModel();
            _vm.Kinect.Canvas = this.canvasControl;
            _vm.Kinect.Initialize();
            _vm.Kinect.Start();
            this.DataContext = _vm;
        }
    }
}
