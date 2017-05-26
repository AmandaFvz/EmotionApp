using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EmotionApp
{
    public class CanvasControl: System.Windows.Controls.Canvas
    {

        public CanvasControl()
        {
            pointBrush = new SolidColorBrush(Colors.Cornsilk);
            boundingBrush = new SolidColorBrush(Colors.LightGray);
            boundingPen = new Pen(boundingBrush, 1);

            NameConverterUtil conv = new NameConverterUtil();
            metricTypeFace = Fonts.GetTypefaces((Uri)conv.Convert("Square", null, "ttf", null)).First();

            Faces = new Dictionary<int, Affdex.Face>();
            MetricNames = new StringCollection();
            upperConverter = new NameUpperCaseConverterUtil();
            appImgs = new Dictionary<string, BitmapImage>();

            maxTxtWidth = 0;
            maxTxtHeight = 0;

            var genderVar = Enum.GetValues(typeof(Affdex.Gender));
            foreach (int genderVal in genderVar)
            {
                for (int g = 0; g <= 1; g++)
                {
                    string name = ConcatInt(genderVal, g);
                    BitmapImage img = loadImage(name);
                    appImgs.Add(name, img);
                }

            }
        }

        public double XScale { get; set; }
        public double YScale { get; set; }

        private const int metricFontSize = 14;
        private double maxTxtWidth;
        private double maxTxtHeight;

        private Typeface metricTypeFace;

        private const int fpRadius = 2;

        private SolidColorBrush pointBrush;
        private SolidColorBrush boundingBrush;
        private Pen boundingPen;
        private NameUpperCaseConverterUtil upperConverter;
        private StringCollection metricNames;

        public Dictionary<int, Affdex.Face> Faces { get; set; }
        public Dictionary<string, string> faceInfo;
        public Dictionary<string, int> eFaceInfo;
        private Dictionary<string, BitmapImage> appImgs;

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            faceInfo = null;
            faceInfo  = new Dictionary<string, string>();
            eFaceInfo = new Dictionary<string, int>();
            
            int count = 0;

            //For each face
            foreach (KeyValuePair<int, Affdex.Face> pair in Faces)
            {
                Affdex.Face face = pair.Value;

                var featurePoints = face.FeaturePoints;

                //Calculate bounding box corners coordinates.
                System.Windows.Point tl = new System.Windows.Point(featurePoints.Min(r => r.X) * XScale,
                                                   featurePoints.Min(r => r.Y) * YScale);
                System.Windows.Point br = new System.Windows.Point(featurePoints.Max(r => r.X) * XScale,
                                                                   featurePoints.Max(r => r.Y) * YScale);

                System.Windows.Point bl = new System.Windows.Point(tl.X, br.Y);

                // Draw points
                foreach (var point in featurePoints)
                {
                    dc.DrawEllipse(pointBrush, null, new System.Windows.Point(point.X * XScale, point.Y * YScale), fpRadius, fpRadius);
                }

                //Draw BoundingBox
                dc.DrawRectangle(null, boundingPen, new System.Windows.Rect(tl, br));

                // Desenha aparencia
                BitmapImage img = appImgs[ConcatInt((int)face.Appearance.Gender, (int)face.Appearance.Glasses)];
                double imgRatio = ((br.Y - tl.Y) * 0.3) / img.Width;
                double imgH = img.Height * imgRatio;
                dc.DrawImage(img, new System.Windows.Rect(br.X + 5, br.Y - imgH, img.Width * imgRatio, imgH));

                double padding = (bl.Y - tl.Y) / MetricNames.Count;
                double startY = tl.Y - padding;
                foreach (string metric in MetricNames)
                {
                    double width = maxTxtWidth;
                    double height = maxTxtHeight;
                    float value = -1;
                    PropertyInfo info = null;
                    if ((info = face.Expressions.GetType().GetProperty(NameMappings(metric))) != null)
                    {
                        value = (float)info.GetValue(face.Expressions, null);
                    }
                    else if ((info = face.Emotions.GetType().GetProperty(NameMappings(metric))) != null)
                    {
                        value = (float)info.GetValue(face.Emotions, null);
                    }

                    value = Math.Abs(value);

                    if (!faceInfo.ContainsKey("Genero: "))
                    {
                        
                        faceInfo.Add("Genero: ", face.Appearance.Gender.ToString().Equals("Female") ? "Feminino" : "Masculino");
                        
                    }
                    
                    if (!faceInfo.ContainsKey("Idade: "))
                    {
                        
                        faceInfo.Add("Idade: ", HelpUtils.getAgeString(face.Appearance.Age.ToString()));
                        
                    }

                    value = Math.Abs(value);

                    if (value > 0)
                    {
                        
                        string[] items = info.ToString().Split(new String[] { " " }, 2, StringSplitOptions.None);
                        string key = HelpUtils.getMetricString(items[1]);

                        
                        if (!eFaceInfo.ContainsKey(key))
                        {
                            eFaceInfo.Add(key, count);
                        }
                        else
                        {
                            eFaceInfo[key] = count++;
                        }
                    }
                }
            }
        }

        private BitmapImage loadImage(string name, string extension = "png")
        {
            NameConverterUtil conv = new NameConverterUtil();
            var pngURI = conv.Convert(name, null, extension, null);
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = (Uri)pngURI;
            img.EndInit();
            return img;
        }

        private string ConcatInt(int x, int y)
        {
            return String.Format("{0}{1}", x, y);
        }

        public String NameMappings(String classifierName)
        {
            if (classifierName == "Frown")
            {
                return "LipCornerDepressor";
            }
            return classifierName;
        }

        public StringCollection MetricNames
        {
            get
            {
                return metricNames;
            }
            set
            {
                metricNames = value;
                Dictionary<string, FormattedText> txtArray = new Dictionary<string, FormattedText>();

                foreach (string metric in metricNames)
                {
                    FormattedText metricFT = new FormattedText((String)upperConverter.Convert(metric, null, null, null),
                                                            System.Globalization.CultureInfo.CurrentCulture,
                                                            System.Windows.FlowDirection.LeftToRight,
                                                            metricTypeFace, metricFontSize, null);
                    txtArray.Add(metric, metricFT);

                }

                if (txtArray.Count > 0)
                {
                    maxTxtWidth = txtArray.Max(r => r.Value.Width);
                    maxTxtHeight = txtArray.Max(r => r.Value.Height);
                }
            }
        }
    }
}
