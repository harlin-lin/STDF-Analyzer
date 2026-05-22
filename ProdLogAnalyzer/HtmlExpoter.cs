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
            if(charts==null)
            {
                charts = new List<BitMap>();
            }
            foreach (var chart in charts)
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
            // 描述表格样式（用于将两行 CSV 输出为表格）
            _reportContent.AppendLine("        .desc-table { width: 100%; border-collapse: collapse; margin-top: 8px; }");
            _reportContent.AppendLine("        .desc-table th, .desc-table td { border: 1px solid #e0e0e0; padding: 8px; text-align: left; font-size: 0.95em; }");
            _reportContent.AppendLine("        .desc-table th { background-color: #f8f8f8; color: #333; }");
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

            // 如果 description 是两行 CSV 格式，则以表格形式输出；否则按原样安全输出
            string descriptionHtml = RenderDescriptionAsTableIfCsv(item.Description);
            _reportContent.AppendLine($"            <div class='item-description {statusClass}'>");
            _reportContent.AppendLine($"                {descriptionHtml}");
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

        /// <summary>
        /// 如果输入是多行 CSV（通常两行），将其渲染成 HTML 表格字符串（已安全转义）。
        /// 否则返回经过 EscapeHtml 的普通文本（并以 &lt;pre&gt; 包裹以保留换行）。
        /// </summary>
        private string RenderDescriptionAsTableIfCsv(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }

            // 按行分割（保留空行）
            var lines = description.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            // 如果只有一行或没有逗号，直接返回转义后的文本（保留换行）
            if (lines.Length < 2)
            {
                return $"<pre>{EscapeHtml(description)}</pre>";
            }

            // 将每行解析为 CSV 字段（支持双引号包含逗号）
            var rows = new List<List<string>>();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    rows.Add(new List<string>());
                }
                else
                {
                    rows.Add(ParseCsvLine(line));
                }
            }

            // 构建 HTML 表格：第一行为表头（如果有），其余为数据行
            var sb = new StringBuilder();
            sb.AppendLine("<table class='desc-table'>");

            // 如果只有两行，通常第一行是 header，第二行为 values
            if (rows.Count >= 2)
            {
                var header = rows[0];
                var second = rows[1];

                // 当列数匹配时，将第一行为 th，第二行为 td；否则把所有行都作为普通行输出
                if (header.Count > 0 && header.Count == second.Count)
                {
                    sb.AppendLine("  <thead>");
                    sb.AppendLine("    <tr>");
                    foreach (var h in header)
                    {
                        sb.AppendLine($"      <th>{EscapeHtml(h)}</th>");
                    }
                    sb.AppendLine("    </tr>");
                    sb.AppendLine("  </thead>");
                    sb.AppendLine("  <tbody>");
                    sb.AppendLine("    <tr>");
                    foreach (var v in second)
                    {
                        sb.AppendLine($"      <td>{EscapeHtml(v)}</td>");
                    }
                    sb.AppendLine("    </tr>");
                    sb.AppendLine("  </tbody>");
                }
                else
                {
                    // 列数不匹配或更复杂，按行输出，列用 td
                    sb.AppendLine("  <tbody>");
                    foreach (var r in rows)
                    {
                        sb.AppendLine("    <tr>");
                        foreach (var c in r)
                        {
                            sb.AppendLine($"      <td>{EscapeHtml(c)}</td>");
                        }
                        sb.AppendLine("    </tr>");
                    }
                    sb.AppendLine("  </tbody>");
                }
            }
            else
            {
                // 少于两行，退回纯文本
                sb.AppendLine("  <tbody>");
                sb.AppendLine("    <tr>");
                sb.AppendLine($"      <td>{EscapeHtml(description)}</td>");
                sb.AppendLine("    </tr>");
                sb.AppendLine("  </tbody>");
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }

        /// <summary>
        /// 解析单行 CSV，支持用双引号包含包含逗号或双引号（双引号按 RFC 行为用两个双引号转义）。
        /// 简单实现，满足常见 CSV 场景。
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (line == null) return result;

            var current = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // 双引号内遇到双引号，检查是否为转义（双双引号）
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++; // 跳过转义的引号
                        }
                        else
                        {
                            inQuotes = false; // 结束引号模式
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }
            result.Add(current.ToString());
            return result;
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
