using PdfSharp.Charting;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionApp
{
    public class PdfBuilder
    {
        public void createPdfInfo(Dictionary<string, string> faceInfo, Dictionary<string, int> eFaceInfo, String screenShotFileName)
        {
            PdfDocument pdf = new PdfDocument();

            PdfPage pdfPage = pdf.AddPage();

            XPen pen = new XPen(XColor.FromName("black"));
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XFont titleFont = new XFont("Calibri", 30, XFontStyle.Bold);
            XFont sectionFont = new XFont("Calibri", 14, XFontStyle.Bold);
            XFont bodyFont = new XFont("Calibri", 11, XFontStyle.Italic);
            XImage logo = XImage.FromFile("C:\\Users\\Vinicius\\Documents\\repoTCC\\EmotionApp\\EmotionApp\\img\\logoEmotionAppWaterMark.PNG");

            String picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            XImage scheenShot = XImage.FromFile(screenShotFileName);
            

            String reportTitle = String.Format("Relatório {0:MMMM/dd/yyyy}", DateTime.Now);

            double width = logo.PointWidth;
            double height = logo.PointHeight;
            double ratio = width / height;
            if (width > pdfPage.Width.Point * 0.5)
            {
                width = pdfPage.Width.Point * 0.5;
                height = width / ratio;
            }
            else if (height > pdfPage.Height.Point * 0.5)
            {
                height = pdfPage.Height.Point * 0.5;
                width = height * ratio;
            }

            double offsetX = (pdfPage.Width.Point - width) / 2;
            double offsetY = (pdfPage.Height.Point - height) / 2;

            XRect rectLogo = new XRect(offsetX, offsetY, width, height);
            graph.DrawImage(logo, rectLogo);

            double position = 25;

            graph.DrawLine(pen, 5, position, pdfPage.Width - 5, position);

            graph.DrawString(reportTitle, titleFont, XBrushes.Black, 
                new XRect(0, 0, pdfPage.Width.Point, position + 40), XStringFormats.BottomCenter);

            graph.DrawLine(pen, 5, position + 50, pdfPage.Width - 5, position + 50);

            foreach (KeyValuePair<string, string> entry in faceInfo)
            {
                if(entry.Key.Contains("Genero"))
                {
                    graph.DrawString(entry.Key, sectionFont, XBrushes.Black,
                        20, position + 75);
                    graph.DrawString(entry.Value, bodyFont, XBrushes.Black,
                        40, position + 100);

                }

                if (entry.Key.Contains("Idade"))
                {
                    graph.DrawString(entry.Key, sectionFont, XBrushes.Black,
                        20, position + 125);
                    graph.DrawString(entry.Value, bodyFont, XBrushes.Black,
                        40, position + 150);

                }
            }

            graph.DrawLine(pen, 5, position + 175, (pdfPage.Width / 2) - 5, position + 175);

            graph.DrawString("Emoções/Expressões analisadas: ", sectionFont, XBrushes.Black, 20, position + 200);

            int i = 0;
            List<string> xseries = new List<string>();
            List<double> series = new List<double>();
            foreach (KeyValuePair<string, int> eEntry in eFaceInfo)
            {
               // int count = 0;
                //foreach (int value in eEntry.Value)
               // {
                    //count += value;
               // }

                xseries.Add(eEntry.Key);
                series.Add((double)eEntry.Value);

                graph.DrawString(eEntry.Key + ": " + eEntry.Value.ToString(), bodyFont, XBrushes.Black,
                40, (position + 225) + i);

                i += 12;
            }

            graph.DrawImage(scheenShot, pdfPage.Width / 2, position + 100);

            graph.DrawString("Gráfico: ", sectionFont, XBrushes.Black, 25, position + 325);

            //Chart chart = PieChart(xseries, series);
            //ChartFrame chartFrame = new ChartFrame();
            //chartFrame.Location = new XPoint(15, position + 350);
            ///chartFrame.Size = new XSize(250, 250);
            //chartFrame.Add(chart);

            ///chartFrame.Draw(graph);

            String pdfFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String fileName = String.Format("EmotionApp_Report_{0:MMMM_dd_yyyy_h_mm_ss}.pdf", DateTime.Now);
            fileName = System.IO.Path.Combine(pdfFolder, fileName);
            
            pdf.Save(fileName);
        }

       private static Chart PieChart(List<string> xseriesList, List<double> seriesList)
        {
            // Set chart type to Pie2D
            Chart chart = new Chart(ChartType.Pie2D);
            
            // Add series data
            Series series = chart.SeriesCollection.AddSeries();
            series.Add(new double[] {seriesList[0], seriesList[1], seriesList[2], seriesList[3], seriesList[4], seriesList[5], seriesList[6] });

            foreach(string val in xseriesList)
            {
                // Add series legend
                XSeries xseries = chart.XValues.AddXSeries();
                xseries.Add(xseriesList[0], xseriesList[1], xseriesList[2], xseriesList[3], xseriesList[4], xseriesList[5], xseriesList[6]);
                chart.Legend.Docking = DockingType.Right;
            }

            // Set label display type
            chart.DataLabel.Type = DataLabelType.Percent;

            // Set label location
            chart.DataLabel.Position = DataLabelPosition.Center;

            return chart;
        }
    }
}
