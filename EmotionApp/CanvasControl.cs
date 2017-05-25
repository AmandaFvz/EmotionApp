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
            maxTxtWidth = 0;
            maxTxtHeight = 0;
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
        public Dictionary<string, List<int>> eFaceInfo;

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            faceInfo = new Dictionary<string, string>();
            eFaceInfo = new Dictionary<string, List<int>>();

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

                double padding = (bl.Y - tl.Y) / MetricNames.Count;
                double startY = tl.Y - padding;
                bool gender = false;
                bool age = false;
                int count = 1;
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

                    if(!gender)
                    {
                        
                        faceInfo.Add("Genero: ", face.Appearance.Gender.ToString());
                        gender = true;
                        
                    }

                    if (!age)
                    {
                       
                        faceInfo.Add("Idade: ", face.Appearance.Age.ToString());
                        age = true;
                        
                    }

                    if (info != null)
                    {
                        string[] items = info.ToString().Split(new String[] { " " }, 2, StringSplitOptions.None); 
                        if (!eFaceInfo.ContainsKey(items[1]))
                        {
                            eFaceInfo.Add(items[1], new List<int> { count++ });
                        }
                        else
                        {
                            eFaceInfo[items[1]].Add(count);
                        }
                    }
                }
            }
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
