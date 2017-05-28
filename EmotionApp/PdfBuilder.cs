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
            XFont bodyFont = new XFont("Calibri", 15, XFontStyle.Regular);
            XImage templateRel = XImage.FromFile("C:\\Users\\Vinicius\\Documents\\repoTCC\\EmotionApp\\EmotionApp\\img\\EmotionApp.png");

            String picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            XImage scheenShot = XImage.FromFile(screenShotFileName);

            graph.DrawImage(templateRel, 0, 0, pdfPage.Width, pdfPage.Height);

            double position = 300;
            

            foreach (KeyValuePair<string, string> entry in faceInfo)
            {
                if(entry.Key.Contains("Genero"))
                {
                    graph.DrawString(entry.Value, bodyFont, XBrushes.Black,
                        80, position);

                }

                if (entry.Key.Contains("Idade"))
                {
                    graph.DrawString(entry.Value, bodyFont, XBrushes.Black,
                        80, position + 75);

                }
            }

            graph.DrawImage(scheenShot, (pdfPage.Width / 2) - 25, position - 105);

            int i = 0;
            List<string> xseries = new List<string>();
            List<double> series = new List<double>();
            if(eFaceInfo.GetEnumerator().MoveNext())
            {
                foreach (KeyValuePair<string, int> eEntry in eFaceInfo)
                {
                    xseries.Add(eEntry.Key);
                    series.Add((double)eEntry.Value);

                    graph.DrawString("- " + eEntry.Key, bodyFont, XBrushes.Black,
                    50, (position + 250) + i);

                    i += 35;
                }

                Chart chart = pieChart(xseries, series);
                ChartFrame chartFrame = new ChartFrame();
                chartFrame.Location = new XPoint((pdfPage.Width / 2) - 60, position + 230);
                chartFrame.Size = new XSize(300, 250);
                chartFrame.Add(chart);

                chartFrame.Draw(graph);
            } 
            else
            {
                // Emocoes
                graph.DrawString("- Nenhuma emoção ", bodyFont, XBrushes.Black,
                    50, (position + 250) + i);
                graph.DrawString("detectada", bodyFont, XBrushes.Black,
                    40, (position + 250) + 20);

                // Grafico
                graph.DrawString("- Nenhuma emoção detectada", bodyFont, XBrushes.Black,
                    (pdfPage.Width / 2) - 60, (position + 250) + i);
            }

            String pdfFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String fileName = String.Format("EmotionApp_Report_{0:MMMM_dd_yyyy_h_mm_ss}.pdf", DateTime.Now);
            fileName = System.IO.Path.Combine(pdfFolder, fileName);
            
            pdf.Save(fileName);
        }

       private static Chart pieChart(List<string> xseriesList, List<double> seriesList)
        {
            
            // Set chart type to Pie2D
            Chart chart = new Chart(ChartType.Pie2D);

            // Add series data
            Series series = chart.SeriesCollection.AddSeries();
            foreach (double val in seriesList)
            {
                series.Add(val);
            }

            // Add series legend
            XSeries xseries = chart.XValues.AddXSeries();
            foreach(string val in xseriesList)
            {
                xseries.Add(val);
            }

            chart.Legend.Docking = DockingType.Right;

            // Set label display type
            chart.DataLabel.Type = DataLabelType.Percent;

            // Set label location
            chart.DataLabel.Position = DataLabelPosition.Center;

            return chart;
        }
    }
}
