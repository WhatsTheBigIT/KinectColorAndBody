using ColorAndDepth.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorAndDepth.ViewModels
{

    public class MainPageViewModel : INotifyPropertyChanged
    {
        public KinectService Kinect { get; set; }

        private string _distance;
        public string Distance
        {
            get { return _distance; }
            set
            {
                _distance = value;
                OnProperyChanged();
            }
        }

        public MainPageViewModel()
        {
            Kinect = new KinectService();
            Kinect.KinectEvent += Kinect_KinectEvent;
        }

        void Kinect_KinectEvent(object sender)
        {
            Distance = Kinect.Depth;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProperyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
