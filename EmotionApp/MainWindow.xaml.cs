using System;
using Affdex;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.Management;

namespace EmotionApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Affdex.ImageListener, Affdex.ProcessStatusListener
    {

        private Affdex.Detector Detector { get; set; }

        private StringCollection EnabledClassifiers { get; set; }

        private int DrawSkipCount { get; set; }

        private bool ShowFacePoints { get; set; }
        private bool ShowMetrics { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Detector = null;

            AddClassifiers();

            // mostra o logo
            logoBackground.Visibility = Visibility.Visible;

            canvas.MetricNames = EnabledClassifiers;

            // habilita/desabilita botões
            btnStopCamera.IsEnabled = btnExit.IsEnabled = true;

            btnStartCamera.IsEnabled = false;

            btnStartCamera.Click += btnStartCamera_Click;
            btnStopCamera.Click += btnStopCamera_Click;
            btnExit.Click += btnExit_Click;

            this.ContentRendered += MainWindow_ContentRendered;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCameraProcessing();
            Application.Current.Shutdown();
        }

        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            StartCameraProcessing();
        }

        private void StartCameraProcessing()
        {
            try
            {
                btnStartCamera.IsEnabled = false;
                btnStopCamera.IsEnabled = 
                btnExit.IsEnabled = true;

                const int cameraId = 0; // usar 10
                const int numberOfFaces = 1;
                const int cameraFPS = 30;
                const int processFPS = 30;

                Detector = new Affdex.CameraDetector(cameraId, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.SMALL_FACES);

                Detector.setClassifierPath("C:\\Program Files\\Affectiva\\AffdexSDK\\data");

                TurnOnClassifiers();

                Detector.setImageListener(this);
                Detector.setProcessStatusListener(this);

                Detector.start();

                cameraDisplay.Visibility = Visibility.Visible;
                logoBackground.Visibility = Visibility.Hidden;
                canvas.Visibility = Visibility.Visible;
            }
            catch (Affdex.AffdexException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    // If this is a camera failure, then reset the application to allow the user to turn on/enable camera
                    if (ex.Message.Equals("Unable to open webcam."))
                    {
                        MessageBoxResult result = MessageBox.Show(ex.Message,
                                                                "EmotionApp Error",
                                                                MessageBoxButton.OK,
                                                                MessageBoxImage.Error);
                        StopCameraProcessing();
                        return;
                    }
                }

                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        private void StopCameraProcessing()
        {
            try
            {
                if ((Detector != null) && (Detector.isRunning()))
                {
                    Detector.stop();
                    Detector.Dispose();
                    Detector = null;
                }

                if(canvas.faceInfo.GetEnumerator().MoveNext())
                {
                    PdfBuilder pdfBuilder = new PdfBuilder();
                    pdfBuilder.createPdfInfo(canvas.faceInfo, canvas.eFaceInfo);
                }

                btnStartCamera.IsEnabled = true;
                btnStopCamera.IsEnabled = false;

            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void AddClassifiers()
        {
            EnabledClassifiers = new StringCollection();

            EnabledClassifiers.Add("Sadness");
            EnabledClassifiers.Add("Anger");
            EnabledClassifiers.Add("Disgust");
            EnabledClassifiers.Add("Surprise");
            EnabledClassifiers.Add("Fear");
            EnabledClassifiers.Add("Smile");
            EnabledClassifiers.Add("Smirk");
        }

        private void TurnOnClassifiers()    
        {
            // Habilitando classifiers

            Detector.setDetectAllEmotions(false);
            Detector.setDetectAllExpressions(false);
            Detector.setDetectGender(true);
            Detector.setDetectAge(true);

            //Detector.setDetectAttention(true);
            //Detector.setDetectBrowFurrow(true);
            //Detector.setDetectBrowRaise(true);
            //Detector.setDetectChinRaise(true);
            //Detector.setDetectEyeClosure(true);
            //Detector.setDetectInnerBrowRaise(true);
            //Detector.setDetectLipPress(true);
            //Detector.setDetectLipPucker(true);
            //Detector.setDetectLipSuck(true);
            //Detector.setDetectMouthOpen(true);
            //Detector.setDetectNoseWrinkle(true);
            Detector.setDetectSmile(true);
            Detector.setDetectSmirk(true);
            //Detector.setDetectUpperLipRaise(true);

            //Detector.setDetectJoy(true);
            Detector.setDetectSadness(true);
            Detector.setDetectAnger(true);
            Detector.setDetectDisgust(true);
            Detector.setDetectSurprise(true);
            Detector.setDetectFear(true);
            Detector.setDetectValence(true);
            Detector.setDetectContempt(true);

        }

        private void ShowExceptionAndShutDown(String exceptionMessage)
        {
            MessageBoxResult result = MessageBox.Show(exceptionMessage,
                                                        "EmotionApp Error",
                                                        MessageBoxButton.OK,
                                                        MessageBoxImage.Error);
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                StopCameraProcessing();
                ResetDisplayArea();
            }));
        }

        private void ResetDisplayArea()
        {
            try
            {
                // Hide Camera feed;
                cameraDisplay.Visibility = Visibility.Hidden;

                // Clear any drawn data
                canvas.Faces = new Dictionary<int, Affdex.Face>();
                canvas.InvalidateVisual();

                // Show the logo
                logoBackground.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        private void DrawCapturedImage(Affdex.Frame image)
        {
            // Update the Image control from the UI thread
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    // Update the Image control from the UI thread
                    //cameraDisplay.Source = rtb;
                    cameraDisplay.Source = ConstructImage(image.getBGRByteArray(), image.getWidth(), image.getHeight());

                    canvas.Faces = new Dictionary<int, Affdex.Face>();
                    canvas.InvalidateVisual();
                    DrawSkipCount = 0;

                    // Allow N successive OnCapture callbacks before the FacePoint drawing canvas gets cleared.
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                    ShowExceptionAndShutDown(message);
                }
            }));
        }

        private void DrawData(Affdex.Frame image, Dictionary<int, Affdex.Face> faces)
        {
            try
            {
                // Plot Face Points
                if (faces != null)
                {
                    var result = this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ((Detector != null) && (Detector.isRunning()))
                        {
                            
                            canvas.Faces = faces;
                            canvas.Width = cameraDisplay.ActualWidth;
                            canvas.Height = cameraDisplay.ActualHeight;
                            canvas.XScale = canvas.Width / image.getWidth();
                            canvas.YScale = canvas.Height / image.getHeight();
                            canvas.InvalidateVisual();
                            DrawSkipCount = 0;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        private BitmapSource ConstructImage(byte[] imageData, int width, int height)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    return imageSrc;
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }

            return null;
        }

        public void TakeScreenShot(String fileName, double width, double height)
        {
            this.Measure(this.RenderSize);
            this.Arrange(new Rect(this.RenderSize));

            Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
            double dpi = 96d;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height,
                                                                       dpi, dpi, PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(this);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            renderBitmap.Render(dv);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(fileName))
            {
                encoder.Save(file);
            }
        }

        private void btnStartCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartCameraProcessing();
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        private void btnStopCamera_Click(object sender, RoutedEventArgs e)
        {
            StopCameraProcessing();
            ResetDisplayArea();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void onImageResults(Dictionary<int, Face> faces, Affdex.Frame image)
        {
            DrawData(image, faces);
        }

        public void onImageCapture(Affdex.Frame image)
        {
            DrawCapturedImage(image);
        }

        public void onProcessingException(AffdexException ex)
        {
            String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp error encountered." : ex.Message;
            ShowExceptionAndShutDown(message);
        }

        public void onProcessingFinished() { }
    }
}
