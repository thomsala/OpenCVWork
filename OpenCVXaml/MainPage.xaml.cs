using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OpenCVXaml.Resources;
using System.Windows.Media.Imaging;
using OpenCVComponent;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Devices;
using System.Threading;

namespace OpenCVXaml
{
    public partial class MainPage : PhoneApplicationPage
    {
        private OpenCVLib m_opencv = new OpenCVLib();
        PhotoCamera cam = new PhotoCamera();
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
        private WriteableBitmap bitmap;
        private Thread ARGBFramesThread;
        private Thread YCRCBFramesThread;
        private bool pumpARGBFrames;
        private bool pumpYCRCBFrames;
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {


            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                // Initialize the default camera.
                cam = new Microsoft.Devices.PhotoCamera(CameraType.FrontFacing);

                //Event is fired when the PhotoCamera object has been initialized
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                //Set the VideoBrush source to the camera
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                // The camera is not supported on the phone.
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    //  txtDebug.Text = "A Camera is not available on this phone.";
                });

                // Disable UI.
                //
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose of the camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
            }
        }

        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // txtDebug.Text = "Camera initialized";
                });

            }
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            pumpARGBFrames = true;


          //  ARGBFramesThread = new System.Threading.Thread(PumpARGBFrames);


            bitmap = new WriteableBitmap((int)cam.PreviewResolution.Width, (int)cam.PreviewResolution.Height);
           // this.MainImage.Source = wb;

            // Start pump.
           // ARGBFramesThread.Start();
            PumpARGBFrames();           
            //if (Preview.Source != null)
            //{
            //    ProcessButton.IsEnabled = false;
                
            //    // Get WriteableBitmap. ImageToModify is defined in MainPage.xaml
            //    WriteableBitmap bitmap = new WriteableBitmap(Preview.Source as BitmapSource);

            //    // call OpenCVLib to convert pixels to grayscale. This is an asynchronous call.
            //   // var pixels =  await m_opencv.ProcessAsync(bitmap.Pixels, bitmap.PixelWidth, bitmap.PixelHeight);
            //    var positions = await m_opencv.FindPositionAsync(bitmap.Pixels, bitmap.PixelWidth, bitmap.PixelHeight);
            //    var pixels = await m_opencv.FindRedAsync(bitmap.Pixels, bitmap.PixelWidth, bitmap.PixelHeight);

            //    // copy the pixels into the WriteableBitmap
            //    for (int x = 0; x < bitmap.Pixels.Length; x++)
            //    {
            //        bitmap.Pixels[x] = pixels[x];
            //    }


            //    // Set Image object, defined in XAML, to the modified bitmap.
            //    Preview.Source = bitmap;


            //    Rectangle rect = new Rectangle();
            //    rect.Fill = new SolidColorBrush(Colors.Green);
            //    rect.StrokeThickness = 2.0;
            //    rect.Width = 30;
            //    rect.Height = 30;
            //    Canvas.SetLeft(rect, positions[0]);
            //    Canvas.SetTop(rect, positions[1]);
            //    canvas.Children.Add(rect);

            //    ProcessButton.IsEnabled = true;
            //}
        }

        public static WriteableBitmap WriteableBitmapCrop(WriteableBitmap wbSource, Int32 offsetX, Int32 offsetY, Int32 outWidth, Int32 outHeight)
        {
            var wbTarget = new WriteableBitmap(outWidth, outHeight);

            for (int x = 0; x < outWidth; x++)
                for (int y = 0; y < outHeight; y++)
                    wbTarget.Pixels[outWidth * y + x] = wbSource.Pixels[outWidth * (y + offsetY) + x + offsetX];

            wbTarget.Invalidate();
            return wbTarget;
        }

        async void PumpARGBFrames()
        {
            // Create capture buffer.
            int[] ARGBPx = new int[(int)cam.PreviewResolution.Width * (int)cam.PreviewResolution.Height];

            try
            {
                PhotoCamera phCam = (PhotoCamera)cam;

                while (pumpARGBFrames)
                {
                    pauseFramesEvent.WaitOne();

                    // Copies the current viewfinder frame into a buffer for further manipulation.
                    phCam.GetPreviewBufferArgb32(ARGBPx);

                
                       // Copy to WriteableBitmap.
                       ARGBPx.CopyTo(bitmap.Pixels, 0);
                       
                   //   bitmap =  WriteableBitmapCrop(bitmap,0,0,480,480);



                   
                        //ProcessButton.IsEnabled = false;

                        //// Get WriteableBitmap. ImageToModify is defined in MainPage.xaml
                        //WriteableBitmap bitmap = new WriteableBitmap(Preview.Source as BitmapSource);

                        // call OpenCVLib to convert pixels to grayscale. This is an asynchronous call.
                        // var pixels =  await m_opencv.ProcessAsync(bitmap.Pixels, bitmap.PixelWidth, bitmap.PixelHeight);
                        var positions = await m_opencv.FindPositionAsync(bitmap.Pixels, bitmap.PixelWidth, bitmap.PixelHeight);
                        var pixels = await m_opencv.FindRedAsync(bitmap.Pixels, bitmap.PixelWidth, bitmap.PixelHeight);

                        // copy the pixels into the WriteableBitmap
                        for (int x = 0; x < bitmap.Pixels.Length; x++)
                        {
                            bitmap.Pixels[x] = pixels[x];
                        }


                        // Set Image object, defined in XAML, to the modified bitmap.
                        Preview.Source = bitmap;


                        Rectangle rect = new Rectangle();
                        rect.Fill = new SolidColorBrush(Colors.Green);
                        rect.StrokeThickness = 2.0;
                        rect.Width = 30;
                        rect.Height = 30;
                        Canvas.SetLeft(rect, positions[0]);
                        Canvas.SetTop(rect, positions[1]);
                        canvas.Children.Add(rect);

                        //ProcessButton.IsEnabled = true;
                    
                   
            
                }

            }
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Display error message.
                    // txtDebug.Text = e.Message;
                });
            }
        }

      
    }
}