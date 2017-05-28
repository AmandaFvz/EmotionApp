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
using System.Windows.Threading;

namespace EmotionApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Affdex.ImageListener, Affdex.ProcessStatusListener
    {

        private Affdex.Detector Detector { get; set; }

        private StringCollection classifiersEnable { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            centerWindow();
        }

        private void winLoaded(object sender, RoutedEventArgs e)
        {
            Detector = null;

            addClassifiers();

            // mostra o logo
            logo.Visibility = Visibility.Visible;

            canvas.MetricNames = classifiersEnable;

            // habilita/desabilita botões
            btnStopCamera.IsEnabled = btnExit.IsEnabled = true;

            btnStartCamera.IsEnabled = false;

            btnStartCamera.Click += btnStartCamera_Click;
            btnStopCamera.Click += btnStopCamera_Click;
            btnExit.Click += btnExit_Click;

            this.ContentRendered += startCamera;
        }

        private void winClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stopCamera();
            Application.Current.Shutdown();
        }

        private void startCamera(object sender, EventArgs e)
        {
            try
            {
                btnStartCamera.IsEnabled = false;
                btnStopCamera.IsEnabled = 
                btnExit.IsEnabled = true;

                const int cameraId = 0; // usar 1
                const int numberOfFaces = 1;
                const int cameraFPS = 60;
                const int processFPS = 60;

                Detector = new Affdex.CameraDetector(cameraId, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.SMALL_FACES);

                Detector.setClassifierPath("C:\\Program Files\\Affectiva\\AffdexSDK\\data");

                setClassifiers();

                Detector.setImageListener(this);
                Detector.setProcessStatusListener(this);

                Detector.start();

                camera.Visibility = Visibility.Visible;
                logo.Visibility = Visibility.Hidden;
                canvas.Visibility = Visibility.Visible;

            }
            catch (Affdex.AffdexException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    if (ex.Message.Equals("Não foi possível abrir a webcam."))
                    {
                        MessageBoxResult result = MessageBox.Show(ex.Message, "EmotionApp Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        stopCamera();
                        return;
                    }
                }

                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                getExceptions(message);
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                getExceptions(message);
            }
        }

        private void stopCamera()
        {
            try
            {
                if ((Detector != null) && (Detector.isRunning()))
                {
                    Detector.stop();
                    Detector.Dispose();
                    Detector = null;
                }

                btnStartCamera.IsEnabled = true;
                btnStopCamera.IsEnabled = false;

                

            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                getExceptions(message);
            }
        }

        private void takeScreenShot(String fileName)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
            double dpi = 96d;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(370, 300, dpi, dpi, PixelFormats.Default);
            Size size = new Size(370, 300);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(this);
                dc.DrawRectangle(vb, null, new Rect(new Point(), size));
            }

            renderBitmap.Render(dv);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(fileName))
            {
                encoder.Save(file);
            }
        }

        private void centerWindow()
        {
            double scrWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double scrHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double winWidth = this.Width;
            double winHeight = this.Height;
            this.Left = (scrWidth / 2) - (winWidth / 2);
            this.Top = (scrHeight / 2) - (winHeight / 2);
        }

        private void addClassifiers()
        {
            classifiersEnable = new StringCollection();

            classifiersEnable.Add("Sadness");
            classifiersEnable.Add("Anger");
            classifiersEnable.Add("Disgust");
            classifiersEnable.Add("Surprise");
            classifiersEnable.Add("Fear");
            classifiersEnable.Add("Joy");
        }

        private void setClassifiers()    
        {
            // Habilitando classifiers
            Detector.setDetectAllEmotions(false);
            Detector.setDetectAllExpressions(false);
            Detector.setDetectGender(true);
            Detector.setDetectAge(true);
            
            Detector.setDetectJoy(true);
            Detector.setDetectSadness(true);
            Detector.setDetectAnger(true);
            Detector.setDetectDisgust(true);
            Detector.setDetectSurprise(true);
            Detector.setDetectFear(true);

        }

        private void getExceptions(String exMsg)
        {
            MessageBoxResult result = MessageBox.Show(exMsg, "EmotionApp Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                stopCamera();
                resetDisplay();
            }));
        }

        private void resetDisplay()
        {
            try
            {
                camera.Visibility = Visibility.Hidden;
                
                canvas.Faces = new Dictionary<int, Affdex.Face>();
                canvas.InvalidateVisual();
                
                logo.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                String msg = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                getExceptions(msg);
            }
        }

        private void drawCapturedImage(Affdex.Frame img)
        {
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    camera.Source = constructImg(img.getBGRByteArray(), img.getWidth(), img.getHeight());

                    canvas.Faces = new Dictionary<int, Affdex.Face>();
                    canvas.InvalidateVisual();
                    
                    if (img != null)
                    {
                        img.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                    getExceptions(message);
                }
            }));
        }

        private void drawData(Affdex.Frame image, Dictionary<int, Affdex.Face> faces)
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
                            canvas.Width = camera.ActualWidth;
                            canvas.Height = camera.ActualHeight;
                            canvas.XScale = canvas.Width / image.getWidth();
                            canvas.YScale = canvas.Height / image.getHeight();
                            canvas.InvalidateVisual();
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                getExceptions(message);
            }
        }

        private BitmapSource constructImg(byte[] imageData, int width, int height)
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
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                getExceptions(message);
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
                startCamera(sender, e);
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
                getExceptions(message);
            }
        }

        private void btnStopCamera_Click(object sender, RoutedEventArgs e)
        {
            String picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            String fileName = String.Format("EmotionApp_ScreenShot_{0:MMMM_dd_yyyy_h_mm_ss}.png", DateTime.Now);
            fileName = System.IO.Path.Combine(picturesFolder, fileName);
            this.takeScreenShot(fileName);

            if (canvas.faceInfo.GetEnumerator().MoveNext() || canvas.eFaceInfo.GetEnumerator().MoveNext())
            {
                PdfBuilder pdfBuilder = new PdfBuilder();
                pdfBuilder.createPdfInfo(canvas.faceInfo, canvas.eFaceInfo, fileName);
            }

            stopCamera();
            resetDisplay();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void onImageResults(Dictionary<int, Face> faces, Affdex.Frame image)
        {
            drawData(image, faces);
        }

        public void onImageCapture(Affdex.Frame image)
        {
            drawCapturedImage(image);
        }

        public void onProcessingException(AffdexException ex)
        {
            String message = String.IsNullOrEmpty(ex.Message) ? "EmotionApp encontrou um erro." : ex.Message;
            getExceptions(message);
        }

        public void onProcessingFinished() { }
    }
}
