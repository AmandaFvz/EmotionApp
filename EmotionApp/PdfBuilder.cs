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
        public void createPdfInfo(Dictionary<string, string> faceInfo, Dictionary<string, List<int>> eFaceInfo)
        {
            PdfDocument pdf = new PdfDocument();

            PdfPage pdfPage = pdf.AddPage();

            XPen pen = new XPen(XColor.FromName("black"));
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XFont titleFont = new XFont("Calibri", 35, XFontStyle.Bold);
            XFont bodyFont = new XFont("Calibri", 14, XFontStyle.Regular);

            XImage logo = XImage.FromFile("C:\\Users\\Vinicius\\Downloads\\Desktop\\TCC_V3\\EmotionApp\\EmotionApp\\img\\logoEmotionAppWaterMark.PNG");
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

            graph.DrawLine(pen, 5, 25, pdfPage.Width - 5, 25);

            graph.DrawString("Relatório", titleFont, XBrushes.Black, 
                new XRect(0, 0, pdfPage.Width.Point, 25 * 4), XStringFormats.Center);

            foreach(KeyValuePair<string, string> entry in faceInfo)
            {
                if(entry.Key.Contains("Genero"))
                {
                    graph.DrawString(entry.Key + entry.Value, bodyFont, XBrushes.Black,
                        10, 35 * 4);
                    
                }

                if (entry.Key.Contains("Idade"))
                {
                    graph.DrawString(entry.Key + entry.Value, bodyFont, XBrushes.Black,
                        10, 45 * 4);

                }
            }

            graph.DrawString("Emoções/Expressões analisadas: ", bodyFont, XBrushes.Black, 10, 55 * 4);

            int i = 0;
            foreach (KeyValuePair<string, List<int>> eEntry in eFaceInfo)
            {
                int count = 0;
                foreach (int value in eEntry.Value)
                {
                    count += value;
                }

                graph.DrawString(eEntry.Key + ": " + count.ToString(), bodyFont, XBrushes.Black,
                30, (60 * 4) + i);

                i += 12;
            }

            String pdfFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String fileName = String.Format("EmotionApp_Report_{0:MMMM_dd_yyyy_h_mm_ss}.pdf", DateTime.Now);
            fileName = System.IO.Path.Combine(pdfFolder, fileName);
            
            pdf.Save(fileName);
        }
    }
}
