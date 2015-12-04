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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App1
{
    public enum DisplayFrameType
    {
        Infrared,
        Color
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
        private DisplayFrameType currentDFT;
        private string statusText;
        private ushort[] irData;
        private byte[] irDataConverted;

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
            
            setupCurrentDisplay(DEFAULT_DISPLAYFRAMETYPE);

            reader = sensor.OpenMultiSourceFrameReader(
                FrameSourceTypes.Infrared | FrameSourceTypes.Color);
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
                switch (currentDFT)
                {
                    case DisplayFrameType.Infrared:
                        using (InfraredFrame infraredFrame =
                            multiSourceFrame.InfraredFrameReference.AcquireFrame())
                        {
                            showInfraredFrame(infraredFrame);
                        }
                        break;

                    case DisplayFrameType.Color:
                        using (ColorFrame colorFrame =
                            multiSourceFrame.ColorFrameReference.AcquireFrame())
                        {
                            showColorFrame(colorFrame);
                        }
                        break;

                    default:
                        break;
                }
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
                    FrameDescription colorFD =
                        sensor.ColorFrameSource.FrameDescription;
                    CurrentFD = colorFD;

                    // Create the bitmap to display
                    bitmap = new WriteableBitmap(colorFD.Width, colorFD.Height);
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
    }
}
