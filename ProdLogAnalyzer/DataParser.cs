using DataContainer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProdLogAnalyzer
{
    static class DataParser
    {
        static bool engMode = false;
        static bool reportExport = false;
        static IDataAcquire dataAcquire = null;
        static int filterId_raw = -1;
        static int filterId_pass = -1;

        const double ConsistencyFactor = 1.4826;
        const double MadOutlierThreshold = 3.5;

        public static void ParseDataFile(ProdLogConfiguration prodLogConfiguration, IDataAcquire da, int filter_raw, int filter_pass, string outputPath)
        {
            HtmlExpoter exporter = null;
            engMode = prodLogConfiguration.EngMode;
            reportExport = prodLogConfiguration.ReportExport;

            dataAcquire = da;
            filterId_pass = filter_pass;
            filterId_raw = filter_raw;

            if (reportExport) exporter = new HtmlExpoter();

            StringBuilder logExporter = new StringBuilder();

            try
            {
                Console.WriteLine("\n========== 生产测试数据分析系统 ==========");

                // 创建PPT文档
                Console.WriteLine($"输出文件: {outputPath}\n");
                if (reportExport)
                {
                    exporter?.CreateReport(outputPath, "生产测试数据分析报告", $"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }

                GenerateDataStatisticReport(exporter, logExporter);

                if (engMode)
                {
                    string summaryPath = Path.Combine(Path.GetDirectoryName(outputPath), $"{Path.GetFileNameWithoutExtension(outputPath)}_Summary.log");
                    File.WriteAllText(summaryPath, logExporter.ToString());
                    logExporter.Clear();

                    logExporter.AppendLine($"TestID,TestText,HiLimit,LoLimit,有效数据量,良率,平均值,标准差,CPK,偏度,超值峰度,Jarque-Bera p,密度峰个数,离群点数量,SiteGap_Mean,SiteGap_Cpk");
                } else {
                    logExporter.AppendLine($"TestID,TestText,HiLimit,LoLimit,有效数据量,良率,平均值,标准差,CPK,偏度,超值峰度,密度峰个数,离群点数量");
                }

                foreach (var id in da.GetTestIDs())
                {
                    if (!engMode)
                    {
                        if (prodLogConfiguration.TargetItemsAndRule == null)
                        {
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.WriteLine($"Prod模式设定下, 配置文件中未指定目标项目和规则!!!!!!!!!!!!!!!!!!!!!!!!");
                        } else
                        {
                            if (!prodLogConfiguration.TargetItemsAndRule.ContainsKey(id))
                            {
                                continue;
                            }
                        }
                    }

                    ItemRuleConfig rule = prodLogConfiguration.GeneralRule;

                    bool forceAna = false;
                    if ((prodLogConfiguration.TargetItemsAndRule != null && prodLogConfiguration.TargetItemsAndRule.ContainsKey(id)))
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

                    // 执行数据分析并生成报告
                    AnalyzeAndGenerateReport(exporter, logExporter, id.ToString(), rule, forceAna);
                }
                // 保存报告
                exporter?.SaveReport();
                string csvPath = Path.Combine(Path.GetDirectoryName(outputPath), $"{Path.GetFileNameWithoutExtension(outputPath)}_Statistic.csv");
                File.WriteAllText(csvPath, logExporter.ToString());
                Console.WriteLine($"\n文件已保存: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n生成报告失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// 生成数据概览幻灯片
        /// </summary>
        private static bool GenerateDataStatisticReport(HtmlExpoter exporter, StringBuilder logExporter)
        {
            var statistic = dataAcquire.GetPartStatistic();

            //if(engMode)
            //{
            //    StringBuilder sb_basic = new StringBuilder();
            //    StringBuilder sb_stastic = new StringBuilder();
            //    StringBuilder sb_sb = new StringBuilder();
            //    StringBuilder sb_hb = new StringBuilder();

            //    SummaryHelper.AppendBasicInfo(ref sb_basic, dataAcquire);
            //    SummaryHelper.AppendCounters(ref sb_stastic, statistic);
            //    SummaryHelper.AppendSoftbin(ref sb_sb, dataAcquire, statistic);
            //    SummaryHelper.AppendHardbin(ref sb_hb, dataAcquire, statistic);

            //    exporter?.AddSummarySlide("Basic Info", sb_basic.ToString());
            //    exporter?.AddSummarySlide("测试数量统计", sb_stastic.ToString());
            //    exporter?.AddSummarySlide("SoftBin统计", sb_sb.ToString());
            //    exporter?.AddSummarySlide("HardBin统计", sb_hb.ToString());

            //    logExporter.Append(sb_basic).Append(sb_stastic).Append(sb_sb).Append(sb_hb);

            //} else
            //{
            //    StringBuilder sb = new StringBuilder();
            //    SummaryHelper.AppendHardbinSimple(ref sb, dataAcquire, statistic);
            //    exporter?.AddSummarySlide("HardBin统计", sb.ToString());

            //    var hbNames = dataAcquire.GetHBinInfo();
            //    //输出statistic中HardBin信息到csv文件中
            //    logExporter.AppendLine("HardBin,Name,P/F,Count,Ratio");
            //    logExporter.AppendLine(
            //        string.Join("\n", statistic.HardBin.Select(kv => $"{kv.Key},{hbNames[kv.Key].Item1},{hbNames[kv.Key].Item2},{kv.Value},{(kv.Value * 100.0 / statistic.TotalCnt)}%"))
            //    );
            //}

            Console.WriteLine("\nFile Summary log done");

            return true;
        }
        
        private static bool checkDataAbnormal(ItemInfo info, NormalityDeviationResult result, ItemStatistic itemStatistic, ItemRuleConfig itemRule)
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
        /// 分析测试数据并生成报告
        /// </summary>
        private static bool AnalyzeAndGenerateReport(HtmlExpoter exporter, StringBuilder logExporter, string testId, ItemRuleConfig itemRule, bool forceAna)
        {
            var info = dataAcquire.GetTestInfo(testId);

            var itemStatistic_raw = dataAcquire.GetFilteredStatistic(filterId_raw, testId);
            var itemStatistic_pass = dataAcquire.GetFilteredStatistic(filterId_pass, testId);
            var data_pass = dataAcquire.GetFilteredItemData(testId, filterId_pass);
            var xs_pass = dataAcquire.GetFilteredPartIndex(filterId_pass);


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
                //Console.WriteLine("NormalityDeviationDetector:" + s.ElapsedMilliseconds);


                if (engMode)
                {
                    //输出测试结果到csv
                    //logExporter.AppendLine($"TestID,TestText,ValidCount,MeanValue,Sigma,Cpk,Yield,偏度,超值峰度,Jarque-Bera p,密度峰个数,离群点数量,SiteGap_Mean,SiteGap_Cpk");
                    logExporter.AppendLine($"{testId},{info.TestText},{info.HiLimit},{info.LoLimit},{itemStatistic_raw.ValidCount},{100.0 * itemStatistic_raw.PassCount / itemStatistic_raw.ValidCount:F4}%,{itemStatistic_pass.MeanValue:F6},{itemStatistic_pass.Sigma:F6},{itemStatistic_pass.Cpk:F4}," +
                    $"{anomalyAnalysis.Skewness:F3},{anomalyAnalysis.ExcessKurtosis:F3},{anomalyAnalysis.JarqueBeraPValue:F4}," +
                        $"{anomalyAnalysis.ModeCount}," +
                        $"{anomalyAnalysis.Outliers.Count}");
                    var anaRst = checkDataAbnormal(info, anomalyAnalysis, itemStatistic_raw, itemRule);

                    if (exporter != null && (!anaRst || forceAna))
                    {
                        s.Restart();
                        //var chartImages = GenerateCharts(data_pass, xs_pass, info, testId, itemStatistic_pass);

                        //exporter?.GenerateReport(testId, info.TestText, chartImages,
                        //    itemStatistic_raw, anomalyAnalysis);
                        s.Stop();
                        //Console.WriteLine($"AddAnalysisSlide: {s.ElapsedMilliseconds} ms");
                        // 输出分析结果到控制台
                        //PrintAnalysisResults(testId, info, itemStatistic_raw, anomalyAnalysis);
                    }

                } 
                else
                {
                    if (forceAna)
                    {
                        //输出测试结果到csv
                        //logExporter.AppendLine($"TestID,TestText,HiLimit,LoLimit,有效数据量,良率,平均值,标准差,CPK,偏度,超值峰度,密度峰个数,离群点数量");
                        logExporter.AppendLine($"{testId},{info.TestText},{info.HiLimit},{info.LoLimit},{itemStatistic_raw.ValidCount},{100.0 * itemStatistic_raw.PassCount / itemStatistic_raw.ValidCount:F4}%,{itemStatistic_pass.MeanValue:F6},{itemStatistic_pass.Sigma:F6},{itemStatistic_pass.Cpk:F4}," +
                        $"{anomalyAnalysis.Skewness:F3},{anomalyAnalysis.ExcessKurtosis:F3}," +
                        $"{anomalyAnalysis.ModeCount}," +
                        $"{anomalyAnalysis.Outliers.Count}");

                        s.Restart();
                        var anaRst = checkDataAbnormal(info, anomalyAnalysis, itemStatistic_raw, itemRule);

                        var chartImages = GenerateCharts(testId);

                        string description = $"Limit:[{info.LoLimit} : {info.HiLimit}] {info.Unit}\n" +
                            $"样本: {itemStatistic_raw.ValidCount} 良率={100.0 * itemStatistic_raw.PassCount / itemStatistic_raw.ValidCount:F4}\n" + 
                            $"均值={itemStatistic_pass.MeanValue:F4} 标准差={itemStatistic_pass.Sigma:F4} Cpk={itemStatistic_pass.Cpk:F4}\n\n" +
                            $"偏度={anomalyAnalysis.Skewness:F3}, 超值峰度={anomalyAnalysis.ExcessKurtosis:F3}\n" + 
                            $"Jarque-Bera p={anomalyAnalysis.JarqueBeraPValue:F4}\n" +
                            $"密度峰个数={anomalyAnalysis.ModeCount} 聚落数量={anomalyAnalysis.Clusters.Count}\n"+
                            $"DBSCAN离群点数量={anomalyAnalysis.Outliers.Count}\n";
                        var status = anaRst ? TestStatus.Pass : TestStatus.Warning;

                        exporter?.GenerateReport($"测试项目: {testId} - {info.TestText}", description, status, chartImages);
                        
                        s.Stop();
                        //Console.WriteLine($"AddAnalysisSlide: {s.ElapsedMilliseconds} ms");
                        // 输出分析结果到控制台
                        //PrintAnalysisResults(testId, info, itemStatistic_raw, anomalyAnalysis);
                    }
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
        private static List<BitMap> GenerateCharts(string testId)
        {
            var charts = new List<BitMap>();
            var info = dataAcquire.GetTestInfo(testId);
            string chartTitle = $"{testId} - {info.TestText}";

            try
            {
                var data_pass = dataAcquire.GetFilteredItemData(testId, filterId_pass);
                var xs_pass = dataAcquire.GetFilteredPartIndex(filterId_pass);
                var itemStatistic_pass = dataAcquire.GetFilteredStatistic(filterId_pass, testId);
                float min_pass = info.LoLimit != null ? info.LoLimit.Value : itemStatistic_pass.MeanValue - 6 * itemStatistic_pass.Sigma;
                float max_pass = info.HiLimit != null ? info.HiLimit.Value : itemStatistic_pass.MeanValue + 6 * itemStatistic_pass.Sigma;

                if (!engMode)
                {
                    var data_raw = dataAcquire.GetFilteredItemData(testId, filterId_raw);
                    var xs_raw = dataAcquire.GetFilteredPartIndex(filterId_raw);
                    var itemStatistic_raw = dataAcquire.GetFilteredStatistic(filterId_raw, testId);
                    float min_raw = info.LoLimit != null ? info.LoLimit.Value : itemStatistic_raw.MeanValue - 6 * itemStatistic_raw.Sigma;
                    float max_raw = info.HiLimit != null ? info.HiLimit.Value : itemStatistic_raw.MeanValue + 6 * itemStatistic_raw.Sigma;

                    charts.Add(new BitMap(ChartGenerator.GenerateComparisonTrendChart(data_raw, xs_raw, data_pass, xs_pass, chartTitle), testId, chartTitle));

                    var boxPara_raw = new BoxPlotPara(
                                    itemStatistic_raw.MedianValue - (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_raw.MAD), 
                                    itemStatistic_raw.MedianValue - itemStatistic_raw.MAD, 
                                    itemStatistic_raw.MedianValue, 
                                    itemStatistic_raw.MedianValue + itemStatistic_raw.MAD, 
                                    itemStatistic_raw.MedianValue + (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_raw.MAD), 
                                    0);
                    charts.Add(new BitMap(ChartGenerator.GenerateHistogram(data_raw, boxPara_raw, info.LoLimit, info.HiLimit, $"Raw: {chartTitle}", min_raw, max_raw), testId, chartTitle));

                    var sites = dataAcquire.GetSites();
                    List<BoxPlotPara> boxParas_bySite = new List<BoxPlotPara>();
                    for (int i = 0; i < sites.Length; i++)
                    {
                        var site = sites[i];
                        var itemStatistic_site = dataAcquire.GetFilteredStatisticBySite(filterId_pass, testId, site);
                        var boxPara_site = new BoxPlotPara(
                                        itemStatistic_site.MedianValue - (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_site.MAD),
                                        itemStatistic_site.MedianValue - itemStatistic_site.MAD,
                                        itemStatistic_site.MedianValue,
                                        itemStatistic_site.MedianValue + itemStatistic_site.MAD,
                                        itemStatistic_site.MedianValue + (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_site.MAD),
                                        itemStatistic_site.ValidCount * sites.Length * 0.9 / itemStatistic_pass.ValidCount);
                        boxParas_bySite.Add(boxPara_site);
                    }
                    charts.Add(new BitMap(ChartGenerator.GenerateBySiteBoxPlot(boxParas_bySite, info.LoLimit, info.HiLimit, chartTitle, min_pass, max_pass), testId, chartTitle));

                    var boxPara_pass = new BoxPlotPara(
                                    itemStatistic_pass.MedianValue - (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_pass.MAD),
                                    itemStatistic_pass.MedianValue - itemStatistic_pass.MAD,
                                    itemStatistic_pass.MedianValue,
                                    itemStatistic_pass.MedianValue + itemStatistic_pass.MAD,
                                    itemStatistic_pass.MedianValue + (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_pass.MAD),
                                    0);
                    charts.Add(new BitMap(ChartGenerator.GenerateHistogram(data_pass, boxPara_pass, info.LoLimit, info.HiLimit, $"Pass: {chartTitle}", min_pass, max_pass), testId, chartTitle));


                } else
                {

                    charts.Add(new BitMap(ChartGenerator.GenerateTrendChart(data_pass, xs_pass, chartTitle), testId, chartTitle));

                    var boxPara_pass = new BoxPlotPara(
                                    itemStatistic_pass.MedianValue - (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_pass.MAD),
                                    itemStatistic_pass.MedianValue - itemStatistic_pass.MAD,
                                    itemStatistic_pass.MedianValue,
                                    itemStatistic_pass.MedianValue + itemStatistic_pass.MAD,
                                    itemStatistic_pass.MedianValue + (float)(MadOutlierThreshold * ConsistencyFactor * itemStatistic_pass.MAD),
                                    0);
                    charts.Add(new BitMap(ChartGenerator.GenerateHistogram(data_pass, boxPara_pass, info.LoLimit, info.HiLimit, chartTitle, min_pass, max_pass), testId, chartTitle));



                }

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
