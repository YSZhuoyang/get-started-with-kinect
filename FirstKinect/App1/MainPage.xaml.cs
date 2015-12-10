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
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;
using System.ComponentModel;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Kinect2Sample;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App1
{
    public enum DisplayFrameType
    {
        Infrared,
        Color,
        Depth,
        BodyMask,
        BodyJoints
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private const int BYTESPERPIXEL = 4;
        private const DisplayFrameType DEFAULT_DISPLAYFRAMETYPE = DisplayFrameType.Infrared;

        private KinectSensor sensor;
        private MultiSourceFrameReader reader;
        private WriteableBitmap bitmap;
        private FrameDescription currentFD;

        // Work as a helper to address the problem that 
        // resolutions of color feeds and depth feeds are different
        private CoordinateMapper coordinateMapper;

        private BodiesManager bodiesManager;
        private Canvas drawingCnavas;

        private DisplayFrameType currentDFT;
        private string statusText;

        // Infrared data
        private ushort[] irData;
        private byte[] irDataConverted;

        // Depth data
        private ushort[] depthData;
        private byte[] depthPixels;

        // BodyMask Frames
        private DepthSpacePoint[] colorMappedToDepthPoints;

        public event PropertyChangedEventHandler PropertyChanged;

        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        public FrameDescription CurrentFD
        {
            get
            {
                return this.currentFD;
            }
            set
            {
                if (this.currentFD != value)
                {
                    this.currentFD = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, 
                            new PropertyChangedEventArgs("CurrentFD"));
                    }
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += mainpage_loaded;
        }

        private void mainpage_loaded(object sender, RoutedEventArgs e)
        {
            sensor = KinectSensor.GetDefault();
            coordinateMapper = sensor.CoordinateMapper;

            setupCurrentDisplay(DEFAULT_DISPLAYFRAMETYPE);

            reader = sensor.OpenMultiSourceFrameReader(
                FrameSourceTypes.Infrared | 
                FrameSourceTypes.Color | 
                FrameSourceTypes.Depth | 
                FrameSourceTypes.BodyIndex | 
                FrameSourceTypes.Body);
            reader.MultiSourceFrameArrived += multiSourceFrameArrived;
            
            // Set Is available changed event notifier
            sensor.IsAvailableChanged += this.isAvailableChanged;

            // Use the window obj as the view model
            this.DataContext = this;

            sensor.Open();
        }
                
        private void multiSourceFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs args)
        {
            MultiSourceFrame multiSourceFrame = args.FrameReference.AcquireFrame();
            
            if (multiSourceFrame != null)
            {
                DepthFrame depthFrame = null;
                ColorFrame colorFrame = null;
                InfraredFrame infraredFrame = null;
                BodyIndexFrame bodyIndexFrame = null;
                BodyFrame bodyFrame = null;

                IBuffer depthFrameDataBuffer = null;
                IBuffer bodyIndexFrameDataBuffer = null;

                // COM interface for unsafe byte manipulation
                IBufferByteAccess bodyIndexByteAccess = null;
                
                switch (currentDFT)
                {
                    case DisplayFrameType.Infrared:
                        using (infraredFrame =
                            multiSourceFrame.InfraredFrameReference.AcquireFrame())
                        {
                            showInfraredFrame(infraredFrame);
                        }
                        break;

                    case DisplayFrameType.Color:
                        using (colorFrame =
                            multiSourceFrame.ColorFrameReference.AcquireFrame())
                        {
                            showColorFrame(colorFrame);
                        }
                        break;

                    case DisplayFrameType.Depth:
                        using (depthFrame = 
                            multiSourceFrame.DepthFrameReference.AcquireFrame())
                        {
                            showDepthFrame(depthFrame);
                        }
                        break;

                    case DisplayFrameType.BodyMask:
                        // Put it in a try catch to utilise finally() and
                        // clean up frames
                        try
                        {
                            depthFrame =
                                multiSourceFrame.DepthFrameReference.AcquireFrame();
                            bodyIndexFrame =
                                multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                            colorFrame =
                                multiSourceFrame.ColorFrameReference.AcquireFrame();

                            if (depthFrame == null ||
                                colorFrame == null ||
                                bodyIndexFrame == null)
                            {
                                return;
                            }

                            // Access the depth frame data directly via
                            // LockImageBuffer to avoid making a copy
                            depthFrameDataBuffer = depthFrame.LockImageBuffer();
                            coordinateMapper.MapColorFrameToDepthSpaceUsingIBuffer(
                                depthFrameDataBuffer,
                                colorMappedToDepthPoints);

                            // Process color
                            colorFrame.CopyConvertedFrameDataToBuffer(
                                bitmap.PixelBuffer, 
                                ColorImageFormat.Bgra);

                            // Access the body index frame data directly via
                            // LockImageBuffer to avoid making a copy
                            bodyIndexFrameDataBuffer = bodyIndexFrame.LockImageBuffer();
                            showMappedBodyFrame(depthFrame.FrameDescription.Width,
                                depthFrame.FrameDescription.Height,
                                bodyIndexFrameDataBuffer, 
                                bodyIndexByteAccess);
                        }
                        finally
                        {
                            if (depthFrame != null)
                            {
                                depthFrame.Dispose();
                            }

                            if (colorFrame != null)
                            {
                                colorFrame.Dispose();
                            }

                            if (bodyIndexFrame != null)
                            {
                                bodyIndexFrame.Dispose();
                            }

                            if (depthFrameDataBuffer != null)
                            {
                                System.Runtime.InteropServices.Marshal.
                                    ReleaseComObject(depthFrameDataBuffer);
                            }

                            if (bodyIndexFrameDataBuffer != null)
                            {
                                System.Runtime.InteropServices.Marshal.
                                    ReleaseComObject(bodyIndexFrameDataBuffer);
                            }

                            if (bodyIndexByteAccess != null)
                            {
                                System.Runtime.InteropServices.Marshal.
                                    ReleaseComObject(bodyIndexByteAccess);
                            }
                        }
                        break;

                    case DisplayFrameType.BodyJoints:
                        using (bodyFrame =
                            multiSourceFrame.BodyFrameReference.AcquireFrame())
                        {
                            showBodyJoints(bodyFrame);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void showBodyJoints(BodyFrame bodyFrame)
        {
            Body[] bodies = new Body[sensor.BodyFrameSource.BodyCount];
            bool dataReceived = false;

            if (bodyFrame != null)
            {
                bodyFrame.GetAndRefreshBodyData(bodies);
                dataReceived = true;
            }

            if (dataReceived)
            {
                bodiesManager.UpdateBodiesAndEdges(bodies);
            }
        }

        private void showInfraredFrame(InfraredFrame infraredFrame)
        {
            bool infraredFrameProcessed = false;

            if (infraredFrame != null)
            {
                FrameDescription infraredFrameDescription =
                    infraredFrame.FrameDescription;

                // Verify data and write the new infrared frame data to the display
                // bitmap
                if (((infraredFrameDescription.Width * infraredFrameDescription.Height == 
                    irData.Length) && 
                    (infraredFrameDescription.Width == bitmap.PixelWidth) && 
                    (infraredFrameDescription.Height == bitmap.PixelHeight)))
                {
                    infraredFrame.CopyFrameDataToArray(irData);
                    infraredFrameProcessed = true;
                }
            }

            if (infraredFrameProcessed)
            {
                convertIrDataToPixelData();
                renderPixelArray(irDataConverted);
            }
        }

        private void showColorFrame(ColorFrame colorFrame)
        {
            bool colorFrameProcessed = false;

            if (colorFrame != null)
            {
                FrameDescription colorFD = colorFrame.FrameDescription;

                // Verify data and write the new color frame data to
                // the writeable bitmap
                if ((colorFD.Width == bitmap.PixelWidth) && 
                    colorFD.Height == bitmap.PixelHeight)
                {
                    if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        colorFrame.CopyRawFrameDataToBuffer(
                            bitmap.PixelBuffer);
                    }
                    else
                    {
                        colorFrame.CopyConvertedFrameDataToBuffer(
                            bitmap.PixelBuffer, ColorImageFormat.Bgra);
                    }

                    colorFrameProcessed = true;
                }
            }

            if (colorFrameProcessed)
            {
                bitmap.Invalidate();
                image.Source = bitmap;
            }
        }

        private void showDepthFrame(DepthFrame depthFrame)
        {
            bool depthFrameProcessed = false;
            ushort minDepth = 0;
            ushort maxDepth = 0;

            if (depthFrame != null)
            {
                FrameDescription depthFD = 
                    depthFrame.FrameDescription;

                // Verify data and write the new infrared frame data 
                // to the display bitmap
                if ((depthFD.Width * depthFD.Height == depthData.Length) && 
                        (depthFD.Width == bitmap.PixelWidth) && 
                        (depthFD.Height == bitmap.PixelHeight))
                {
                    depthFrame.CopyFrameDataToArray(depthData);

                    minDepth = depthFrame.DepthMinReliableDistance;
                    maxDepth = depthFrame.DepthMaxReliableDistance;

                    depthFrameProcessed = true;
                }
            }

            if (depthFrameProcessed)
            {
                convertDepthDataToPixels(minDepth, maxDepth);
                renderPixelArray(depthPixels);
            }
        }

        unsafe private void showMappedBodyFrame(int depthWidth, 
            int depthHeight, IBuffer bodyIndexFrameDataBuffer, 
            IBufferByteAccess bodyIndexByteAccess)
        {
            bodyIndexByteAccess = (IBufferByteAccess)bodyIndexFrameDataBuffer;
            byte *bodyIndexBytes = null;
            bodyIndexByteAccess.Buffer(out bodyIndexBytes);

            fixed (DepthSpacePoint *colorMappedToDepthPointsPointer = 
                colorMappedToDepthPoints)
            {
                IBufferByteAccess bitmapBackBufferByteAccess =
                    (IBufferByteAccess)bitmap.PixelBuffer;
                byte* bitmapBackBufferBytes = null;
                bitmapBackBufferByteAccess.Buffer(out bitmapBackBufferBytes);

                // Treat color data as 4 bytes pixels
                uint* bitmapPixelsPointer = (uint*)bitmapBackBufferBytes;

                // Loop over each row and column of color image
                // Zero out any pixels that don't correspond to a body index
                int colorMappedLength = colorMappedToDepthPoints.Length;

                for (int colorIndex = 0; colorIndex < colorMappedLength; ++colorIndex)
                {
                    float colorMappedToDepthX =
                        colorMappedToDepthPointsPointer[colorIndex].X;
                    float colorMappedToDepthY =
                        colorMappedToDepthPointsPointer[colorIndex].Y;
                    
                    // The sentinel value is -inf, -inf, 
                    // meaning that no depth pixel corresponds to this color pixel
                    if (!float.IsNegativeInfinity(colorMappedToDepthX) && 
                        !float.IsNegativeInfinity(colorMappedToDepthY))
                    {
                        // Make sure depth pixel maps to a valid point in color space
                        int depthX = (int)(colorMappedToDepthX + 0.5f);
                        int depthY = (int)(colorMappedToDepthY + 0.5f);

                        // If the point is not valid, there is no body index
                        // there
                        if ((depthX >= 0) && (depthX < depthWidth) && 
                            (depthY >= 0) && (depthY < depthHeight))
                        {
                            int depthIndex = (depthY * depthWidth) + depthX;
                            //Debug.WriteLine("show");

                            if (bodyIndexBytes[depthIndex] != 0xff)
                            {
                                // This bodyIndexByte is gook and is a body
                                // loop again
                                continue;
                            }
                        }
                    }

                    // This pixel does not correspond to a body, 
                    // make it black and transparent
                    bitmapPixelsPointer[colorIndex] = 0;
                }
            }

            bitmap.Invalidate();
            image.Source = bitmap;
        }

        private void convertDepthDataToPixels(ushort minDepth, ushort maxDepth)
        {
            int colorPixelIndex = 0;
            int mapDepthToByte = maxDepth / 256;

            for (int i = 0; i < depthData.Length; i++)
            {
                ushort depth = depthData[i];

                byte intensity = (byte)(depth >= minDepth && 
                    depth <= maxDepth ? (depth / mapDepthToByte) : 0);
                
                depthPixels[colorPixelIndex++] = intensity;
                depthPixels[colorPixelIndex++] = intensity;
                depthPixels[colorPixelIndex++] = intensity;
                depthPixels[colorPixelIndex++] = 255;
            }
        }

        private void convertIrDataToPixelData()
        {
            for (int i = 0; i < irData.Length; i++)
            {
                // Convert value range from 0 - 65535 to 0 - 255
                byte intensity = (byte)(irData[i] >> 8);

                // Convert it into a grey scale map
                irDataConverted[i * BYTESPERPIXEL] = intensity;
                irDataConverted[i * BYTESPERPIXEL + 1] = intensity;
                irDataConverted[i * BYTESPERPIXEL + 2] = intensity;
                irDataConverted[i * BYTESPERPIXEL + 3] = 255;
            }
        }

        private void renderPixelArray(byte[] pixelData)
        {
            // Copy data to writable bitmap obj for display
            pixelData.CopyTo(bitmap.PixelBuffer);
            bitmap.Invalidate();
            image.Source = bitmap;
        }

        private void setupCurrentDisplay(DisplayFrameType newDFT)
        {
            currentDFT = newDFT;

            // Frames used by more than one type are declared outside the switch
            FrameDescription colorFD;

            if (BodyJointsGrid != null)
            {
                BodyJointsGrid.Visibility = Visibility.Collapsed;
            }

            if (image != null)
            {
                image.Source = null;
            }

            switch (currentDFT)
            {
                case DisplayFrameType.Infrared:
                    FrameDescription inFD = sensor.InfraredFrameSource.FrameDescription;
                    this.CurrentFD = inFD;
                    irData = new ushort[inFD.LengthInPixels];
                    irDataConverted = new byte[inFD.LengthInPixels * BYTESPERPIXEL];

                    // Create the bitmap to display
                    bitmap = new WriteableBitmap(inFD.Width, inFD.Height);
                    break;

                case DisplayFrameType.Color:
                    colorFD = sensor.ColorFrameSource.FrameDescription;
                    CurrentFD = colorFD;

                    // Create the bitmap to display
                    bitmap = new WriteableBitmap(colorFD.Width, colorFD.Height);
                    break;
                    
                case DisplayFrameType.Depth:
                    FrameDescription depthFD = 
                        sensor.DepthFrameSource.FrameDescription;
                    CurrentFD = depthFD;
                    depthData = new ushort[depthFD.Width * depthFD.Height];
                    depthPixels = new byte[depthFD.Width * depthFD.Height * BYTESPERPIXEL];
                    bitmap = new WriteableBitmap(depthFD.Width, depthFD.Height);
                    break;

                case DisplayFrameType.BodyMask:
                    colorFD = sensor.ColorFrameSource.FrameDescription;
                    CurrentFD = colorFD;

                    // Allocate space to put the pixels being received and converted
                    colorMappedToDepthPoints = 
                        new DepthSpacePoint[colorFD.Width * colorFD.Height];
                    bitmap = new WriteableBitmap(
                        colorFD.Width, colorFD.Height);
                    break;

                case DisplayFrameType.BodyJoints:
                    drawingCnavas = new Canvas();

                    drawingCnavas.Clip = new RectangleGeometry();
                    drawingCnavas.Clip.Rect = new Rect(0.0, 0.0, 
                        BodyJointsGrid.Width,
                        BodyJointsGrid.Height);
                    drawingCnavas.Width = BodyJointsGrid.Width;
                    drawingCnavas.Height = BodyJointsGrid.Height;

                    // Reset body joints grid
                    BodyJointsGrid.Visibility = Visibility.Visible;
                    BodyJointsGrid.Children.Clear();

                    // Add canvas to DisplayGrid
                    BodyJointsGrid.Children.Add(drawingCnavas);
                    bodiesManager = new BodiesManager(coordinateMapper,
                        drawingCnavas, sensor.BodyFrameSource.BodyCount);
                    break;

                default:
                    break;
            }
        }

        private void isAvailableChanged(KinectSensor sender, IsAvailableChangedEventArgs args)
        {
            this.StatusText = sensor.IsAvailable ?
                "Running" : "Nor Available";
        }

        private void InfraredButton_Click(object sender, RoutedEventArgs e)
        {
            setupCurrentDisplay(DisplayFrameType.Infrared);
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            setupCurrentDisplay(DisplayFrameType.Color);
        }

        private void DepthButton_Click(object sender, RoutedEventArgs e)
        {
            setupCurrentDisplay(DisplayFrameType.Depth);
        }

        private void BodyMask_Click(object sender, RoutedEventArgs e)
        {
            setupCurrentDisplay(DisplayFrameType.BodyMask);
        }

        private void BodyJointsButton_Click(object sender, RoutedEventArgs e)
        {
            setupCurrentDisplay(DisplayFrameType.BodyJoints);
        }

        [Guid("905a0fef-bc53-11df-8c49-001e4fc686da"),
                 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IBufferByteAccess
        {
            unsafe void Buffer(out byte* pByte);
        }
    }
}
