using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Graphics.Canvas;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace ColorAndDepth.Services
{
    public class KinectService
    {
        //This example uses Microsoft Win2D, which provides a GPU accelerated graphics library and is available with NuGet.  Search for Win2D, if you don't have it.
        //The CanvasControl provides additional functionality for drawing, over the WriteableBitmap.  
        public Microsoft.Graphics.Canvas.CanvasControl Canvas { get; set; }

        public delegate void KinectServiceEvent (object sender);

        public event KinectServiceEvent KinectEvent;


        private CanvasBitmap _bitmap;

        private KinectSensor _sensor;
        private MultiSourceFrameReader _multiReader;
        private byte[] _colorBuffer;

        private Body[] _bodies;
        

        private CoordinateMapper _mapper;

        public string Depth { get; set; }
        public KinectService()
        {

        }
        public void Initialize()
        {
            _sensor = KinectSensor.GetDefault();
            _mapper = _sensor.CoordinateMapper;

            //We only need Color and Body for this example
            _multiReader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color |
                                                                FrameSourceTypes.Body);
                                                                


            _multiReader.MultiSourceFrameArrived += _multiReader_MultiSourceFrameArrived;

            _colorBuffer = new byte[_sensor.ColorFrameSource.FrameDescription.Width *
                                    _sensor.ColorFrameSource.FrameDescription.Height *
                                    4];


            _bodies = new Body[_sensor.BodyFrameSource.BodyCount];
            

            Canvas.Draw += Canvas_Draw;

        }

        /// <summary>
        /// Perform drawing routines in the Canvas_Draw method.  This will be called everytime the control needs to be redrawn.  You can force a redraw by calling Invalidate (see below).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var session = args.DrawingSession)
            {
                //Create a color bitmap from the color data from the sensor
                //The ByteArray needs to be in BGRA format (4 bytes/pixel).

                _bitmap = CanvasBitmap.CreateFromBytes(
                session.Device,
                _colorBuffer,
                _sensor.ColorFrameSource.FrameDescription.Width,
                _sensor.ColorFrameSource.FrameDescription.Height,
                Microsoft.Graphics.Canvas.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                CanvasAlphaMode.Premultiplied, 96);


                ColorSpacePoint[] csJoints = new ColorSpacePoint[Body.JointCount];

                if (_bitmap != null)
                {
                    //Draw the color bitmap
                    args.DrawingSession.DrawImage(_bitmap);
                }

                //Iterate over each body
                foreach (Body body in _bodies)
                {
                    //only process if a body is tracked
                    if (body != null && body.IsTracked)
                    {

                        var joints = (from joint in body.Joints
                                      select joint.Value.Position).ToArray<CameraSpacePoint>();

                        //Map joint from 3D Camera Space, to 2D ColorSpace.
                        _sensor.CoordinateMapper.MapCameraPointsToColorSpace(joints, csJoints);


                        //Now, draw a circle for each joint.
                        foreach (var point in csJoints)
                        {
                            args.DrawingSession.FillCircle(point.X, point.Y, 5.0F, Colors.Blue);
                        }
                        
                    }
                }
                
                //Get the distance of the tip of the right hand of the first body being tracked
                //We can track 6 bodies, but only want to display one of them. It would be no issue to
                //add code and adjust the UI to support all 6 bodies.
                var firstTrackedBody = (from body in _bodies
                                        where body != null && body.IsTracked == true
                                        select body).FirstOrDefault<Body>();
                Depth = " ";

                if (firstTrackedBody != null)
                {
                    if (firstTrackedBody.Joints[JointType.HandTipRight].TrackingState == TrackingState.Tracked)
                    {
                        //Get the Position of the join. This is going to be a camera space value (3D)
                        var handCameraSpace = firstTrackedBody.Joints[JointType.HandTipRight].Position;
                      
                        //Convert Z from Meters to Inches and display value as string 
                        Depth = String.Format(" {0:N3} inches", (handCameraSpace.Z * 39.3701));
                      
                    }

                }

                //Fire the event so the UI is updated with the distance 
                if (KinectEvent != null)
                {
                    KinectEvent(this);
                }
            }
        }

        public void Start()
        {
            _sensor.Open();
        }
        public void Stop()
        {
            _sensor.Close();
        }
        private void _multiReader_MultiSourceFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs args)
        {
            using (var multiFrame = args.FrameReference.AcquireFrame())
            {
                if (multiFrame != null)
                {
                    using (var colorFrame = multiFrame.ColorFrameReference.AcquireFrame())
                    {
                        using (var bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
                        {
                            ProcessData(colorFrame, bodyFrame);
                        }
                    }
                }
            }
        }

        private void ProcessData(ColorFrame cFrame, BodyFrame bFrame)
        {
            if (cFrame != null)
            {
                cFrame.CopyConvertedFrameDataToArray(_colorBuffer, ColorImageFormat.Bgra);
            }
            if (bFrame != null)
            {
                bFrame.GetAndRefreshBodyData(_bodies);
            }
            //This will fire the Draw event for the canvas.  This is when we want to do all of the drawing
            Canvas.Invalidate();

        }
    }
}