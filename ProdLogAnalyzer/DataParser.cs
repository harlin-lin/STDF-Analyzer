using DataContainer;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Linq;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProdLogAnalyzer.StatisticsAnalyzer;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace ProdLogAnalyzer
{
    static class DataParser
    {
        public static void ParseDataFile(ProdLogConfiguration prodLogConfiguration, IDataAcquire da, int filterId_raw, int filterId_pass, string outputPath)
        {
            PowerPointExporter exporter = null;
            if(prodLogConfiguration.SlidesExport) exporter = new PowerPointExporter();

            StringBuilder csvExpoter = new StringBuilder();

            try
            {
                Console.WriteLine("\n========== 生产测试数据分析系统 ==========");

                // 创建PPT文档
                Console.WriteLine($"输出文件: {outputPath}\n");
                if (prodLogConfiguration.SlidesExport)
                {
                    exporter?.CreatePresentation(outputPath, prodLogConfiguration.SlidesTemplatePath, "生产测试数据分析报告", $"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }
                GenerateDataStatisticSlide(exporter, csvExpoter, da, prodLogConfiguration.EngMode);

                foreach (var id in da.GetTestIDs())
                {
                    if (!prodLogConfiguration.EngMode)
                    {
                        if (prodLogConfiguration.TargetItemsAndRule == null)
                        {
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则");
                        } else
                        {
                            if (!prodLogConfiguration.TargetItemsAndRule.ContainsKey(id))
                            {
                                continue;
                            }
                        }
                    }
                    var info = da.GetTestInfo(id);
                    var itemStatistic = da.GetFilteredStatistic(filterId_raw, id);

                    var data_pass = da.GetFilteredItemData(id, filterId_pass);
                    var xs_pass = da.GetFilteredPartIndex(filterId_pass);

                    ItemRuleConfig rule = prodLogConfiguration.GeneralRule;

                    bool forceAna = false;
                    if (prodLogConfiguration.TargetItemsAndRule != null && prodLogConfiguration.TargetItemsAndRule.ContainsKey(id))
                    {
                        forceAna = true;
                        var r = prodLogConfiguration.TargetItemsAndRule[id];
                        if(r.YieldLimit_High != null) rule.YieldLimit_High = r.YieldLimit_High;
                        if(r.YieldLimit_Low != null) rule.YieldLimit_Low = r.YieldLimit_Low;
                        if(r.SigmaLimit_High != null) rule.SigmaLimit_High = r.SigmaLimit_High;
                        if(r.SigmaLimit_Low != null) rule.SigmaLimit_Low = r.SigmaLimit_Low;
                        if(r.CpkLimit_High != null) rule.CpkLimit_High = r.CpkLimit_High;
                        if(r.CpkLimit_Low != null) rule.CpkLimit_Low = r.CpkLimit_Low;
                        if(r.MeanLimit_High != null) rule.MeanLimit_High = r.MeanLimit_High;
                        if(r.MeanLimit_Low != null) rule.MeanLimit_Low = r.MeanLimit_Low;
                        if(r.SkewnessLimit_High != null) rule.SkewnessLimit_High = r.SkewnessLimit_High;
                        if(r.SkewnessLimit_Low != null) rule.SkewnessLimit_Low = r.SkewnessLimit_Low;
                        if(r.ExcessKurtosisLimit_High != null) rule.ExcessKurtosisLimit_High = r.ExcessKurtosisLimit_High;
                        if(r.ExcessKurtosisLimit_Low != null) rule.ExcessKurtosisLimit_Low = r.ExcessKurtosisLimit_Low;
                        if(r.JarqueBeraLimit != null) rule.JarqueBeraLimit = r.JarqueBeraLimit;
                        if(r.ModeCountLimit != null) rule.ModeCountLimit = r.ModeCountLimit;
                        if(r.ClustersLimit != null) rule.ClustersLimit = r.ClustersLimit;
                        if(r.OutliersLimit != null) rule.OutliersLimit = r.OutliersLimit;
                    }

                    // 执行数据分析并生成PPT幻灯片
                    AnalyzeAndGenerateSlide(exporter, csvExpoter, id.ToString(), info, itemStatistic, rule, data_pass, xs_pass, forceAna);
                    //if(dbg++ >= 50)
                    //{
                    //    Console.WriteLine("调试模式: 仅处理前10个TestID");
                    //    break;
                    //}
                }
                // 保存PPT
                exporter?.SaveAndClose();
                string csvPath = Path.Combine(Path.GetDirectoryName(outputPath), $"{Path.GetFileNameWithoutExtension(outputPath)}_Statistic.csv");
                File.WriteAllText(csvPath, csvExpoter.ToString());
                Console.WriteLine($"PPT文件已保存: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ 生成PPT报告失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// 生成数据概览幻灯片
        /// </summary>
        private static bool GenerateDataStatisticSlide(PowerPointExporter exporter, StringBuilder csvExpoter, IDataAcquire da, bool engMode)
        {
            StringBuilder sb = new StringBuilder();
            var statistic = da.GetPartStatistic();

            if(engMode)
            {
                SummaryHelper.AppendBasicInfo(ref sb, da);
                exporter?.AddSummarySlide("Basic Info", sb.ToString());
                Console.WriteLine("\n============================================================");
                Console.Write(sb.ToString());
                Console.WriteLine("\n============================================================");
                sb.Clear();

                SummaryHelper.AppendCounters(ref sb, statistic);
                exporter?.AddSummarySlide("测试数量统计", sb.ToString());
                Console.WriteLine("\n============================================================");
                Console.Write(sb.ToString());
                Console.WriteLine("\n============================================================");
                sb.Clear();

                SummaryHelper.AppendSoftbin(ref sb, da, statistic);
                exporter?.AddSummarySlide("SoftBin统计", sb.ToString());
                Console.WriteLine("\n============================================================");
                Console.Write(sb.ToString());
                Console.WriteLine("\n============================================================");
                sb.Clear();

                SummaryHelper.AppendHardbin(ref sb, da, statistic);
                exporter?.AddSummarySlide("HardBin统计", sb.ToString());
                Console.WriteLine("\n============================================================");
                Console.Write(sb.ToString());
                Console.WriteLine("\n============================================================");
                sb.Clear();
            } else
            {
                SummaryHelper.AppendHardbinSimple(ref sb, da, statistic);
                exporter?.AddSummarySlide("HardBin统计", sb.ToString());
                Console.WriteLine("\n============================================================");
                Console.Write(sb.ToString());
                Console.WriteLine("\n============================================================");
                sb.Clear();

                var hbNames = da.GetHBinInfo();
                //输出statistic中HardBin信息到csv文件中
                csvExpoter.AppendLine("HardBin,Name,P/F,Count,Ratio");
                csvExpoter.AppendLine(
                    string.Join("\n", statistic.HardBin.Select(kv => $"{kv.Key},{hbNames[kv.Key].Item1},{hbNames[kv.Key].Item2},{kv.Value},{(kv.Value * 100.0 / statistic.TotalCnt)}%"))
                );
                csvExpoter.AppendLine($"TestID,TestText,ValidCount,MeanValue,Sigma,Cpk,Yield,偏度,超值峰度,Jarque-Bera p,密度峰个数,离群点数量");
            }

            return true;
        }
        private static bool checkDataNormal(ItemInfo info, NormalityDeviationResult result, ItemStatistic itemStatistic, ItemRuleConfig itemRule)
        {
            //先判断是否有上下限，如果没有上下限则不进行正态性检验，直接认为数据正常
            if (info.HiLimit == null && info.LoLimit == null)
            {
                return true;
            }

            //判断是否良率符合预期, 不符合直接返回不正常
            if (itemRule != null)
            {
                double yield = (double)itemStatistic.PassCount / itemStatistic.ValidCount;
                if(itemRule.YieldLimit_High != null && itemRule.YieldLimit_High <= yield)
                {
                    return false;
                }
                if (itemRule.YieldLimit_Low != null && itemRule.YieldLimit_Low >= yield)
                {
                    return false;
                }
            }

            //如果数据为常数序列，则不进行正态性检验，直接认为数据正常
            if (result.IsConstantData)
            {
                return true;
            }

            if (itemRule != null)
            {
                if(itemRule.SigmaLimit_High != null && itemRule.SigmaLimit_High <= result.StdDev)
                {
                    return false;
                }
                if (itemRule.SigmaLimit_Low != null && itemRule.SigmaLimit_Low >= result.StdDev)
                {
                    return false;
                }
                if(itemRule.CpkLimit_High != null && !double.IsNaN(result.Cpk) && itemRule.CpkLimit_High <= result.Cpk)
                {
                    return false;
                }
                if (itemRule.CpkLimit_Low != null && !double.IsNaN(result.Cpk) && itemRule.CpkLimit_Low >= result.Cpk)
                {
                    return false;
                }
                if(itemRule.MeanLimit_High != null && itemRule.MeanLimit_High <= result.Mean)
                {
                    return false;
                }
                if (itemRule.MeanLimit_Low != null && itemRule.MeanLimit_Low >= result.Mean)
                {
                    return false;
                }
                if(itemRule.SkewnessLimit_High != null && itemRule.SkewnessLimit_High <= result.Skewness)
                {
                    return false;
                }
                if (itemRule.SkewnessLimit_Low != null && itemRule.SkewnessLimit_Low >= result.Skewness)
                {
                    return false;
                }
                if(itemRule.ExcessKurtosisLimit_High != null && itemRule.ExcessKurtosisLimit_High <= result.ExcessKurtosis)
                {
                    return false;
                }
                if (itemRule.ExcessKurtosisLimit_Low != null && itemRule.ExcessKurtosisLimit_Low >= result.ExcessKurtosis)
                {
                    return false;
                }
                if (itemRule.JarqueBeraLimit != null && itemRule.JarqueBeraLimit <= result.JarqueBeraStatistic)
                {
                    return false;
                }
                if (itemRule.ModeCountLimit != null && itemRule.ModeCountLimit < result.ModeCount)
                {
                    return false;
                }
                if (itemRule.ClustersLimit != null && itemRule.ClustersLimit < result.Clusters.Count)
                {
                    return false;
                }
                if (itemRule.OutliersLimit != null && itemRule.OutliersLimit < result.Outliers.Count)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 分析测试数据并生成幻灯片
        /// </summary>
        private static bool AnalyzeAndGenerateSlide(PowerPointExporter exporter, StringBuilder csvExpoter, string testId,
            ItemInfo info, ItemStatistic itemStatistic, ItemRuleConfig itemRule, IEnumerable<float> data_pass, IEnumerable<int> xs_pass, bool forceAna)
        {

            if (info.HiLimit == null && info.LoLimit == null && !forceAna)
            {
                Console.WriteLine($"  跳过(无管控) TestID: {testId} - {info.TestText}");
                return true;
            } 
            else if(info.HiLimit != null && info.LoLimit != null && info.HiLimit == info.LoLimit && !forceAna)
            {
                Console.WriteLine($"  跳过(常数管控) TestID: {testId} - {info.TestText}");
                return true;
            } 
            else
            {
                Console.WriteLine($"  处理 TestID: {testId} - {info.TestText}");
            }

            try
            {
                DeviationAnalysisParams para = new DeviationAnalysisParams
                {
                    SignificanceLevel = 0.05,
                    PeakProminenceRatio = 0.5,
                    DbscanEpsilon = 6,
                    DbscanMinPts = 5,
                    UpperSpecLimit = info.HiLimit,
                    LowerSpecLimit = info.LoLimit
                };

                var s = new System.Diagnostics.Stopwatch();
                s.Start();
                var anomalyAnalysis = NormalityDeviationDetector.Analyze(data_pass, para);
                s.Stop();
                Console.WriteLine("NormalityDeviationDetector:" + s.ElapsedMilliseconds);


                if (!checkDataNormal(info, anomalyAnalysis, itemStatistic, itemRule) || forceAna)
                {
                    if (exporter != null)
                    {
                        s.Restart();
                        var chartImages = GenerateCharts(data_pass, xs_pass, info, testId, itemStatistic);

                        exporter?.AddAnalysisSlide(testId, info.TestText, chartImages,
                            itemStatistic, anomalyAnalysis);
                        s.Stop();
                        Console.WriteLine("AddAnalysisSlide:" + s.ElapsedMilliseconds);
                    }
                    // 输出分析结果到控制台
                    PrintAnalysisResults(testId, info, itemStatistic, anomalyAnalysis);

                    //输出测试结果到csv
                    //csvExpoter.AppendLine($"TestID,TestText,ValidCount,MeanValue,Sigma,Cpk,Yield,偏度,超值峰度,Jarque-Bera p,密度峰个数,离群点数量");
                    csvExpoter.AppendLine($"{testId},{info.TestText},{itemStatistic.ValidCount},{itemStatistic.MeanValue:F6},{itemStatistic.Sigma:F6},{itemStatistic.Cpk:F4},{100.0 * itemStatistic.PassCount / itemStatistic.ValidCount:F4}%," +
                        $"{anomalyAnalysis.Skewness:F3},{anomalyAnalysis.ExcessKurtosis:F3},{anomalyAnalysis.JarqueBeraPValue:F4}," +
                        $"{anomalyAnalysis.ModeCount}," +
                        $"{anomalyAnalysis.Outliers.Count}");

                }


                return true;
            } catch (Exception ex)
            {
                Console.WriteLine($"    ✗ 处理失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 生成6个图表
        /// </summary>
        private static List<Bitmap> GenerateCharts(IEnumerable<float> data, IEnumerable<int> xs, ItemInfo info, string testId, ItemStatistic itemStatistic)
        {
            var charts = new List<Bitmap>();
            string chartTitle = $"{testId} - {info.TestText}";

            try
            {
                float min = info.LoLimit != null ? info.LoLimit.Value : itemStatistic.MeanValue - 6 * itemStatistic.Sigma;
                float max = info.HiLimit != null ? info.HiLimit.Value : itemStatistic.MeanValue + 6 * itemStatistic.Sigma;

                charts.Add(ChartGenerator.GenerateTrendChart(data, xs,
                    "数据趋势"));

                charts.Add(ChartGenerator.GenerateHistogram(data, info.LoLimit, info.HiLimit, "数据分布 (100 bins)", min, max));

                charts.Add(ChartGenerator.GenerateBoxPlot(data, info.LoLimit, info.HiLimit, "数据箱型图", min, max));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ⚠ 图表生成失败: {ex.Message}");
            }

            return charts;
        }

        /// <summary>
        /// 输出分析结果到控制台
        /// </summary>
        private static void PrintAnalysisResults(string testId, ItemInfo info,
            ItemStatistic itemStatistic,
            NormalityDeviationResult result)
        {
            Console.WriteLine($"    原始: {itemStatistic.ValidCount}个 | " +
                $"均值={itemStatistic.MeanValue:F6} | 标准差={itemStatistic.Sigma:F6} | " +
                $"Cpk={itemStatistic.Cpk:F4} | 良率={100.0 * itemStatistic.PassCount/itemStatistic.ValidCount:F4}%");

            Console.WriteLine(result.IsConstantData
                ? "警告：输入数据为常数序列。"
                : $"均值={result.Mean:F3}, 标准差={result.StdDev:F3}");
            Console.WriteLine($"偏度={result.Skewness:F3}, 超值峰度={result.ExcessKurtosis:F3}");
            Console.WriteLine($"Jarque-Bera p={result.JarqueBeraPValue:F4}, 正态性：{result.IsNormal}");
            Console.WriteLine($"密度峰个数={result.ModeCount}, 位置=[{string.Join(", ", result.ModeLocations.Select(x => x.ToString("F2")))}]");
            Console.WriteLine($"聚落数量={result.Clusters.Count}, 大小=[{string.Join(", ", result.ClusterSizes)}]");
            Console.WriteLine($"DBSCAN离群点数量={result.Outliers.Count}");
        }
    }
}
