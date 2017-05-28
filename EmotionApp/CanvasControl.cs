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
            eFaceInfo = new Dictionary<string, int>();
            faceInfo = new Dictionary<string, string>();

            maxTxtWidth = 0;
            maxTxtHeight = 0;

            var genderVar = Enum.GetValues(typeof(Affdex.Gender));
            foreach (int genderVal in genderVar)
            {
                for (int g = 0; g <= 1; g++)
                {
                    string name = ConcatInt(genderVal, g);
                    BitmapImage img = loadImg(name);
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

        int countSmile = 0;
        int countSurprise = 0;
        int countFear = 0;
        int countDisgust = 0;
        int countSad = 0;
        int countAnger = 0;

        int firstTime = 0;
        bool drawApp = true;

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            
            foreach (KeyValuePair<int, Affdex.Face> pair in Faces)
            {
                Affdex.Face face = pair.Value;

                var fPoints = face.FeaturePoints;
                
                System.Windows.Point tl = new System.Windows.Point(fPoints.Min(r => r.X) * XScale, fPoints.Min(r => r.Y) * YScale);
                System.Windows.Point br = new System.Windows.Point(fPoints.Max(r => r.X) * XScale, fPoints.Max(r => r.Y) * YScale);
                System.Windows.Point bl = new System.Windows.Point(tl.X, br.Y);

                foreach (var point in fPoints)
                {
                    dc.DrawEllipse(pointBrush, null, new System.Windows.Point(point.X * XScale, point.Y * YScale), fpRadius, fpRadius);
                }
                
                dc.DrawRectangle(null, boundingPen, new System.Windows.Rect(tl, br));

                // Desenha aparencia
                if(drawApp)
                {
                    BitmapImage img = appImgs[ConcatInt((int)face.Appearance.Gender, (int)face.Appearance.Glasses)];
                    double imgRatio = ((br.Y - tl.Y) * 0.3) / img.Width;
                    double imgH = img.Height * imgRatio;
                    dc.DrawImage(img, new System.Windows.Rect(br.X + 5, br.Y - imgH, img.Width * imgRatio, imgH));

                }


                foreach (string metrica in MetricNames)
                {
                    float valor = -1;
                    PropertyInfo info = null;
                    if ((info = face.Expressions.GetType().GetProperty(NameMap(metrica))) != null)
                    {
                        valor = (float)info.GetValue(face.Expressions, null);
                    }
                    else if ((info = face.Emotions.GetType().GetProperty(NameMap(metrica))) != null)
                    {
                        valor = (float)info.GetValue(face.Emotions, null);
                    }

                    if (faceInfo.ContainsKey("Genero") && firstTime == 0)
                    {

                        faceInfo.Remove("Genero");

                    }

                    if ((int)face.Appearance.Gender > 0)
                    {
                        faceInfo.Add("Genero", (int)face.Appearance.Gender == 2 ? "Feminino" : "Masculino");
                    }

                    if (faceInfo.ContainsKey("Idade") && firstTime == 0)
                    {

                        faceInfo.Remove("Idade");

                    }

                    if (!face.Appearance.Age.ToString().Contains("Unknow") && firstTime == 0)
                    {

                        faceInfo.Add("Idade", HelpUtils.getAgeString(face.Appearance.Age.ToString()));

                    }


                    if (metrica.Equals("Joy") && valor > 99)
                    {

                        countSmile++;

                        string key = HelpUtils.getMetricString(metrica);

                        if(eFaceInfo.ContainsKey(key))
                        {
                            eFaceInfo.Remove(key);

                            eFaceInfo.Add(key, countSmile);
                        } 
                        else
                        {
                            eFaceInfo.Add(key, countSmile);
                        }
                    }

                    if (metrica.Equals("Anger") && valor > 37)
                    {
                        countAnger++;

                        string key = HelpUtils.getMetricString(metrica);

                        if (eFaceInfo.ContainsKey(key))
                        {
                            eFaceInfo.Remove(key);

                            eFaceInfo.Add(key, countAnger);
                        }
                        else
                        {
                            eFaceInfo.Add(key, countAnger);
                        }
                    }

                    if (metrica.Equals("Sadness") && valor > 98)
                    {
                        countSad++;

                        string key = HelpUtils.getMetricString(metrica);

                        if (eFaceInfo.ContainsKey(key))
                        {
                            eFaceInfo.Remove(key);

                            eFaceInfo.Add(key, countSad);
                        }
                        else
                        {
                            eFaceInfo.Add(key, countSad);
                        }
                    }

                    if (metrica.Equals("Surprise") && valor > 96)
                    {
                        countSurprise++;

                        string key = HelpUtils.getMetricString(metrica);

                        if (eFaceInfo.ContainsKey(key))
                        {
                            eFaceInfo.Remove(key);

                            eFaceInfo.Add(key, countSurprise);
                        }
                        else
                        {
                            eFaceInfo.Add(key, countSurprise);
                        }
                    }

                    if (metrica.Equals("Fear") && valor > 66)
                    {
                        countFear++;

                        string key = HelpUtils.getMetricString(metrica);

                        if (eFaceInfo.ContainsKey(key))
                        {
                            eFaceInfo.Remove(key);

                            eFaceInfo.Add(key, countFear);
                        }
                        else
                        {
                            eFaceInfo.Add(key, countFear);
                        }
                    }

                    if (metrica.Equals("Disgust") && valor > 96)
                    {
                        countDisgust++;

                        string key = HelpUtils.getMetricString(metrica);

                        if (eFaceInfo.ContainsKey(key))
                        {
                            eFaceInfo.Remove(key);

                            eFaceInfo.Add(key, countDisgust);
                        }
                        else
                        {
                            eFaceInfo.Add(key, countDisgust);
                        }
                    }
                }
            }
        }

        private BitmapImage loadImg(string name, string extension = "png")
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

        public String NameMap(String classifierName)
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
