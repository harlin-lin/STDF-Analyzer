using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ProdLogAnalyzer
{
    public enum TestStatus
    {
        Pass,
        Warning
    }

    internal class ReportItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TestStatus Status { get; set; }
        public List<BitMap> Charts { get; set; }
    }

    public class HtmlExpoter
    {
        private string _outputPath;
        private string _imageDir;
        private string _title;
        private string _subtitle;
        private StringBuilder _reportContent;

        public HtmlExpoter()
        {
            _reportContent = new StringBuilder();
        }

        public void CreateReport(string outputPath, string title, string subtitle = "")
        {
            _outputPath = outputPath;
            _title = title;
            _subtitle = subtitle;

            _imageDir = Path.Combine(Path.GetDirectoryName(_outputPath), "images");
            if (!Directory.Exists(_imageDir))
            {
                Directory.CreateDirectory(_imageDir);
            }

            // 初始化报告内容
            InitializeReport();
        }

        public void GenerateReport(string title, string description, TestStatus status, List<BitMap> charts)
        {
            foreach(var chart in charts)
            {
                string fileName = $"chart_{chart.TestId}_{Guid.NewGuid().ToString().Substring(0, 8)}.png";
                string filePath = Path.Combine(_imageDir, fileName);
                chart.Data.Save(filePath);
                chart.FileName = fileName;
            }
            var item = new ReportItem
            {
                Title = title,
                Description = description,
                Status = status,
                Charts = charts ?? new List<BitMap>()
            };

            // 添加到报告内容
            AddReportItemToContent(item);
        }

        public void SaveReport()
        {
            if (string.IsNullOrEmpty(_outputPath))
            {
                throw new InvalidOperationException("Output path not specified. Call CreateReport first.");
            }

            // 完成报告内容
            CompleteReport();

            // 确保输出目录存在
            var directory = Path.GetDirectoryName(_outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 保存主报告文件
            File.WriteAllText(_outputPath, _reportContent.ToString(), Encoding.UTF8);
        }

        private void InitializeReport()
        {
            _reportContent.Clear();

            _reportContent.AppendLine("<!DOCTYPE html>");
            _reportContent.AppendLine("<html lang='en'>");
            _reportContent.AppendLine("<head>");
            _reportContent.AppendLine("    <meta charset='UTF-8'>");
            _reportContent.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            _reportContent.AppendLine($"    <title>{EscapeHtml(_title)}</title>");
            _reportContent.AppendLine("    <style>");
            _reportContent.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }");
            _reportContent.AppendLine("        .header { text-align: center; margin-bottom: 30px; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            _reportContent.AppendLine("        .title { color: #333; font-size: 2em; margin: 0; }");
            _reportContent.AppendLine("        .subtitle { color: #666; font-size: 1.2em; margin: 10px 0 0 0; }");
            _reportContent.AppendLine("        .container { max-width: 1200px; margin: 0 auto; }");
            _reportContent.AppendLine("        .report-item { background-color: #fff; margin-bottom: 20px; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            _reportContent.AppendLine("        .item-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }");
            _reportContent.AppendLine("        .item-title { margin: 0; font-size: 1.4em; }");
            _reportContent.AppendLine("        .status-pass { color: #28a745; font-weight: bold; }");
            _reportContent.AppendLine("        .status-fail { color: #dc3545; font-weight: bold; }");
            _reportContent.AppendLine("        .status-warning { color: #ffc107; font-weight: bold; }");
            _reportContent.AppendLine("        .item-description { margin: 15px 0; line-height: 1.6; }");
            // 将图片容器改为两列布局，并在窄屏上自动切换为一列
            _reportContent.AppendLine("        .chart-container { display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; margin-top: 15px; }");
            _reportContent.AppendLine("        .chart { text-align: center; }");
            _reportContent.AppendLine("        .chart img { max-width: 100%; height: auto; border: 1px solid #ddd; border-radius: 4px; }");
            _reportContent.AppendLine("        .chart-caption { margin-top: 8px; font-size: 0.9em; color: #666; }");
            _reportContent.AppendLine("        @media (max-width: 700px) { .chart-container { grid-template-columns: 1fr; } }");
            _reportContent.AppendLine("    </style>");
            _reportContent.AppendLine("</head>");
            _reportContent.AppendLine("<body>");
            _reportContent.AppendLine("    <div class='header'>");
            _reportContent.AppendLine($"        <h1 class='title'>{EscapeHtml(_title)}</h1>");
            if (!string.IsNullOrEmpty(_subtitle))
            {
                _reportContent.AppendLine($"        <p class='subtitle'>{EscapeHtml(_subtitle)}</p>");
            }
            _reportContent.AppendLine("    </div>");
            _reportContent.AppendLine("    <div class='container'>");
        }

        private void AddReportItemToContent(ReportItem item)
        {
            _reportContent.AppendLine("        <div class='report-item'>");
            _reportContent.AppendLine("            <div class='item-header'>");

            string statusClass = GetStatusClass(item.Status);
            string statusText = item.Status.ToString();

            _reportContent.AppendLine($"                <h2 class='item-title {statusClass}'>{EscapeHtml(item.Title)}</h2>");
            _reportContent.AppendLine($"                <span class='{statusClass}'>{statusText}</span>");
            _reportContent.AppendLine("            </div>");

            _reportContent.AppendLine($"            <div class='item-description {statusClass}'>");
            _reportContent.AppendLine($"                {EscapeHtml(item.Description)}");
            _reportContent.AppendLine("            </div>");

            if (item.Charts.Count > 0)
            {
                _reportContent.AppendLine("            <div class='chart-container'>");

                foreach (var chart in item.Charts)
                {
                    string chartFileName = chart.FileName;
                    _reportContent.AppendLine("                <div class='chart'>");
                    // 使用 web-friendly 的相对路径（正斜杠），并保持图片自适应两列布局
                    _reportContent.AppendLine($"                    <img src='images/{chartFileName}' alt='{EscapeHtml(chart.FileName)}'>");
                    if (!string.IsNullOrEmpty(chart.Description))
                    {
                        _reportContent.AppendLine($"                    <div class='chart-caption'>{EscapeHtml(chart.Description)}</div>");
                    }
                    _reportContent.AppendLine("                </div>");
                }

                _reportContent.AppendLine("            </div>");
            }

            _reportContent.AppendLine("        </div>");
        }

        private void CompleteReport()
        {
            _reportContent.AppendLine("    </div>");
            _reportContent.AppendLine("</body>");
            _reportContent.AppendLine("</html>");
        }

        private string GetStatusClass(TestStatus status)
        {
            switch(status)
            {
                case TestStatus.Pass:
                    return "status-pass";
                case TestStatus.Warning:
                    return "status-warning";
                default:
                    return "";
            } 
        }

        private string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;");
        }
    }
}
