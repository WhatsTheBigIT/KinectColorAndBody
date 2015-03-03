using KinectPhotoBooth3.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleColor.ViewModels
{
   
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public KinectService Kinect { get; set; }



        public MainPageViewModel()
        {
            Kinect = new KinectService();
            Kinect.Start();

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
