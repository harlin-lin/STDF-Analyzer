using DataContainer;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Wordprocessing;
using ShapeCrawler;
using ShapeCrawler.Presentations;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace ProdLogAnalyzer
{
    public class PowerPointExporter
    {
        private Presentation _draftPresentation;
        private string _outputPath;
      
        public void CreatePresentation(string filepath, string templatepath, string title, string subtitle = "")
        {
            _outputPath = filepath;

            if(!File.Exists(templatepath))
            {
                Console.WriteLine($"Template file not found: {templatepath}");

            } else
            {
                File.Copy(templatepath, filepath, overwrite: true);
            }

            _draftPresentation = new Presentation(filepath);
            _draftPresentation.Save(_outputPath);
            _draftPresentation.Slides.Add(1);
            var slide = _draftPresentation.Slides.Last();


            var shape = slide.Shapes[1];
            shape.SetText(title);
            shape.SetFontSize(54);

            if (!string.IsNullOrEmpty(subtitle))
            {
                //slide.Shapes.AddShape(300, 0, 200, 100);
                shape = slide.Shapes[2];
                shape.SetText(subtitle);
                shape.SetFontSize(20);
            }
        }

        public void AddSummarySlide(string title, string summary)
        {
            _draftPresentation.Slides.Add(2);
            var slide = _draftPresentation.Slides.Last();

            var shape = slide.Shapes[0];
            shape.SetText(title);
            shape.SetFontSize(24);

            slide.Shapes[1].Remove();

            slide.Shapes.AddTextBox(0, 80, 800, 450, summary);
            shape = slide.Shapes.Last();
            shape.SetFontSize(12);
            shape.TextBox.VerticalAlignment = TextVerticalAlignment.Top;

        }

        public void AddAnalysisSlide(string testId, string testName,
            List<Bitmap> chartImages, ItemStatistic itemStatistic,
            NormalityDeviationResult anomaly)
        {
            _draftPresentation.Slides.Add(2);
            var slide = _draftPresentation.Slides.Last();

            var shape = slide.Shapes[0];
            shape.SetText($"测试项目: {testId} - {testName}");
            shape.SetFontSize(24);

            slide.Shapes[1].Remove();

            if (chartImages != null && chartImages.Count >= 3)
            {
                //int chartWidth = 380;
                //int chartHeight = 160;
                //int leftMargin = 5;
                //int topMargin = 50;
                //int hSpacing = 0;
                //int vSpacing = 0;
                //AddImageToSlide(slide, chartImages[0], leftMargin, topMargin, chartWidth, chartHeight);
                //AddImageToSlide(slide, chartImages[1], leftMargin + chartWidth + hSpacing, topMargin, chartWidth, chartHeight);

                //int row2Top = topMargin + chartHeight + vSpacing;
                //AddImageToSlide(slide, chartImages[2], leftMargin, row2Top, chartWidth, chartHeight);
                //AddImageToSlide(slide, chartImages[3], leftMargin + chartWidth + hSpacing, row2Top, chartWidth, chartHeight);

                //int row3Top = row2Top + chartHeight + vSpacing;
                //AddImageToSlide(slide, chartImages[4], leftMargin, row3Top, chartWidth, chartHeight);
                //AddImageToSlide(slide, chartImages[5], leftMargin + chartWidth + hSpacing, row3Top, chartWidth, chartHeight);

                int chartWidth = 600;
                int chartHeight = 160;
                int leftMargin = 5;
                int topMargin = 50;
                int vSpacing = 0; 
                AddImageToSlide(slide, chartImages[0], leftMargin, topMargin, chartWidth, chartHeight);

                int row2Top = topMargin + chartHeight + vSpacing;
                AddImageToSlide(slide, chartImages[1], leftMargin, row2Top, chartWidth, chartHeight);

                int row3Top = row2Top + chartHeight + vSpacing;
                AddImageToSlide(slide, chartImages[2], leftMargin, row3Top, chartWidth, chartHeight);
            }

            //AddStatisticsTextBox(slide, itemStatistic, anomaly, 75, 770);
            AddStatisticsTextBox(slide, itemStatistic, anomaly, 75, 610);
        }

        public void SaveAndClose()
        {
            if (_draftPresentation != null && !string.IsNullOrEmpty(_outputPath))
            {
                _draftPresentation.Save(_outputPath);
                _draftPresentation = null;
            }
        }

        private void SetSlideBackground(dynamic slide)
        {
            try
            {
                var colorType = typeof(ShapeCrawler.Color);
                var color = Activator.CreateInstance(colorType, new object[] { 255, 255, 255 });
                slide.SolidBackground(color);
            }
            catch
            {
                try
                {
                    var colorType = typeof(ShapeCrawler.Color);
                    var color = Activator.CreateInstance(colorType);
                    var properties = colorType.GetProperties();
                    foreach (var prop in properties)
                    {
                        if (prop.Name.Equals("R", StringComparison.OrdinalIgnoreCase))
                            prop.SetValue(color, 255);
                        else if (prop.Name.Equals("G", StringComparison.OrdinalIgnoreCase))
                            prop.SetValue(color, 255);
                        else if (prop.Name.Equals("B", StringComparison.OrdinalIgnoreCase))
                            prop.SetValue(color, 255);
                    }
                    slide.SolidBackground(color);
                }
                catch
                {
                }
            }
        }

        private void AddStatisticsTextBox(IUserSlide slide, ItemStatistic itemStatistic,
            NormalityDeviationResult anomaly, int top, int left)
        {
            var statsLines = new List<string>
            {
                $"原始: {itemStatistic.ValidCount}个 \n均值={itemStatistic.MeanValue:F4} \n标准差={itemStatistic.Sigma:F4} \nCpk={itemStatistic.Cpk:F4} \n良率={100.0 * itemStatistic.PassCount/itemStatistic.ValidCount:F4}\n\n",

                $"偏度={anomaly.Skewness:F3}, 超值峰度={anomaly.ExcessKurtosis:F3}",
                $"Jarque-Bera p={anomaly.JarqueBeraPValue:F4}",
                $"密度峰个数={anomaly.ModeCount}",
                $"聚落数量={anomaly.Clusters.Count}",
                $"DBSCAN离群点数量={anomaly.Outliers.Count}"
            };

            string statsText = string.Join("\n", statsLines);
            slide.Shapes.AddTextBox(left, top, 200, 450, statsText);
            var shape = slide.Shapes.Last();
            shape.SetFontSize(12);
            shape.TextBox.VerticalAlignment = TextVerticalAlignment.Top;
        }

        private void AddImageToSlide(IUserSlide slide, Bitmap image, int left, int top, int width, int height)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                
                //slide.Picture(memoryStream, left, top, width, height);
                slide.Shapes.AddPicture(memoryStream);
                var shape = slide.Shapes.Last();
                shape.X = left;
                shape.Y = top;
                shape.Width = width;
                shape.Height = height;
            }
        }
    }
}