using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;
using System.Runtime.InteropServices.WindowsRuntime;

namespace KinectPhotoBooth3.Services
{
    public class KinectService
    {
        public WriteableBitmap Image { get; set; }
        private KinectSensor _sensor;
        private ColorFrameReader _colorReader;
        private byte[] _colorBuffer;
        public KinectService()
        {
            Initialize();
        }
        public void Initialize()
        {
            _sensor = KinectSensor.GetDefault();
            _colorReader = _sensor.ColorFrameSource.OpenReader();

            _colorReader.FrameArrived += _colorReader_FrameArrived;

            _colorBuffer = new byte[_sensor.ColorFrameSource.FrameDescription.Width *
                                    _sensor.ColorFrameSource.FrameDescription.Height *
                                    4];

            Image = new WriteableBitmap(_sensor.ColorFrameSource.FrameDescription.Width,
                                        _sensor.ColorFrameSource.FrameDescription.Height);
        }
        public void Start()
        {
            _sensor.Open();
        }
        public void Stop()
        {
            _sensor.Close();
        }
        private void _colorReader_FrameArrived(ColorFrameReader sender, ColorFrameArrivedEventArgs args)
        {
            using (var colorFrame = args.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        colorFrame.CopyRawFrameDataToArray(_colorBuffer);
                    }
                    else
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_colorBuffer,ColorImageFormat.Bgra);
                    }
                    
                    using (Stream stream = Image.PixelBuffer.AsStream())
                    {
                        stream.Write(_colorBuffer, 0, _colorBuffer.Length);
                    }
                    Image.Invalidate();
                }
            }
        }
    }
}
