using DataContainer;
using MathNet.Numerics.Statistics;
using ScottPlot.Statistics;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProdLogAnalyzer
{
    enum ItemAnalysePriority
    {
        General,
        Ignore,
        ForceAnalyse
    }
    static class DataParser
    {
        static bool engMode = false;
        static bool reportExport = false;
        static string csvTitle = string.Empty;
        static IDataAcquire dataAcquire = null;
        static int filterId_raw = -1;
        static int filterId_pass = -1;
        static string outputPath = "";

        const float ConsistencyFactor = 1.4826f;

        public static void ParseDataFile(ProdLogConfiguration prodLogConfiguration, IDataAcquire da, int filter_raw, int filter_pass)
        {
            HtmlExpoter exporter = null;
            engMode = prodLogConfiguration.EngMode;
            reportExport = prodLogConfiguration.ReportExport;

            string lotInfo = string.Empty; 
            if (prodLogConfiguration.LotInfoRegex != string.Empty)
            {
                var match = Regex.Match(prodLogConfiguration.DataFiles[0].Path, prodLogConfiguration.LotInfoRegex);
                if (match.Success) {
                    lotInfo = match.Groups[0].Value + "_";
                }
            }
            var reportBasePath = $"{prodLogConfiguration.Name}_{lotInfo}{DateTime.Now:yyyyMMdd_HHmmss}";
            if(!Directory.Exists(Path.Combine(prodLogConfiguration.OutputFolder, reportBasePath)))
            {
                Directory.CreateDirectory(Path.Combine(prodLogConfiguration.OutputFolder, reportBasePath));
            }

            outputPath = Path.Combine(prodLogConfiguration.OutputFolder, reportBasePath, $"{reportBasePath}.html");

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
                    exporter?.CreateReport(outputPath, $"生产测试数据分析报告_{lotInfo}", $"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }

                GenerateDataStatisticReport(exporter, logExporter);

                csvTitle = $"TestID,TestText,HiLimit,LoLimit,数据量,良率,平均值,标准差,CPK,偏度,峰度,密度峰,离群点,{(engMode ? "Median,MAD_L,MAD_R,SiteGap,结果" : string.Empty)}";
                logExporter.AppendLine(csvTitle);

                foreach (var id in da.GetTestIDs())
                {
                    var testText = dataAcquire.GetTestInfo(id).TestText;
                    ItemAnalysePriority anaflg = ItemAnalysePriority.General;
                    ItemRuleConfig r=null;
                    if (prodLogConfiguration.TargetItemsAndRuleByTestId.ContainsKey(id))
                    {
                        anaflg = ItemAnalysePriority.ForceAnalyse;
                        r = prodLogConfiguration.TargetItemsAndRuleByTestId[id];

                    }
                    foreach(var k in prodLogConfiguration.TargetItemsAndRuleByTestTextRegex.Keys)
                    {

                        if (Regex.IsMatch(testText, k, RegexOptions.IgnoreCase))
                        {
                            anaflg = ItemAnalysePriority.ForceAnalyse;
                            r = prodLogConfiguration.TargetItemsAndRuleByTestTextRegex[k];
                        }
                    }
                    if (!engMode && anaflg != ItemAnalysePriority.ForceAnalyse)
                    {
                        //Console.WriteLine($"  跳过 TestID: {id} - {testText}");
                        continue;
                    }

                    ItemRuleConfig rule = new ItemRuleConfig(prodLogConfiguration.GeneralRule);

                    if (anaflg == ItemAnalysePriority.ForceAnalyse)
                    {
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
                        if(r.ModeCountLimit != null) rule.ModeCountLimit = r.ModeCountLimit;
                        if(r.OutliersLimit != null) rule.OutliersLimit = r.OutliersLimit;

                        if (r.Para_BandwidthRatio != null) rule.Para_BandwidthRatio = r.Para_BandwidthRatio;
                        if (r.Para_KernelSampleCount != null) rule.Para_KernelSampleCount = r.Para_KernelSampleCount;
                        if (r.Para_PeakProminenceRatio != null) rule.Para_PeakProminenceRatio = r.Para_PeakProminenceRatio;
                        if (r.Para_MAD_Threshold_Left != null) rule.Para_MAD_Threshold_Left = r.Para_MAD_Threshold_Left;
                        if (r.Para_MAD_Threshold_Right != null) rule.Para_MAD_Threshold_Right = r.Para_MAD_Threshold_Right;
                        if (r.Para_MAD_Threshold_HalfLimit != null) rule.Para_MAD_Threshold_HalfLimit = r.Para_MAD_Threshold_HalfLimit;
                    }
                    
                    if(anaflg != ItemAnalysePriority.ForceAnalyse)
                    {
                        foreach (var iid in prodLogConfiguration.IgnoredItemsByTestId)
                        {
                            if (id == iid)
                            {
                                anaflg = ItemAnalysePriority.Ignore;
                                break;
                            }
                        }
                        foreach (var pat in prodLogConfiguration.IgnoredItemsByTestTextRegex)
                        {
                            if (Regex.IsMatch(testText, pat, RegexOptions.IgnoreCase))
                            {
                                anaflg = ItemAnalysePriority.Ignore;
                                break;
                            }
                        }
                    }

                    // 执行数据分析并生成报告
                    DeviationAnalysisParams analysisParams = new DeviationAnalysisParams();                    
                    if(rule.Para_BandwidthRatio != null) analysisParams.BandwidthRatio = rule.Para_BandwidthRatio.Value;
                    if(rule.Para_KernelSampleCount != null) analysisParams.KernelSampleCount = rule.Para_KernelSampleCount.Value;
                    if(rule.Para_PeakProminenceRatio != null) analysisParams.PeakProminenceRatio = rule.Para_PeakProminenceRatio.Value;
                    if(rule.Para_MAD_Threshold_Left != null) analysisParams.MadOutlierThRatio_Left = rule.Para_MAD_Threshold_Left.Value;
                    if(rule.Para_MAD_Threshold_Right != null) analysisParams.MadOutlierThRatio_Right = rule.Para_MAD_Threshold_Right.Value;
                    if(rule.Para_MAD_Threshold_HalfLimit != null) analysisParams.MadHalfLimitThRatio = rule.Para_MAD_Threshold_HalfLimit.Value;
                    AnalyzeAndGenerateReport(exporter, logExporter, id.ToString(), rule, anaflg, analysisParams);
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

            var hbNames = dataAcquire.GetHBinInfo();
            //输出statistic中HardBin信息到csv文件中
            logExporter.AppendLine("HardBin,Name,P/F,Count,Ratio");
            logExporter.AppendLine(
                string.Join("\n", statistic.HardBin.Select(kv => $"{kv.Key},{hbNames[kv.Key].Item1},{hbNames[kv.Key].Item2},{kv.Value},{(kv.Value * 100.0 / statistic.TotalCnt)}%"))
            );

            if (engMode)
            {
                StringBuilder sb_basic = new StringBuilder();
                StringBuilder sb_stastic = new StringBuilder();
                StringBuilder sb_sb = new StringBuilder();
                StringBuilder sb_hb = new StringBuilder();

                SummaryHelper.AppendBasicInfo(ref sb_basic, dataAcquire);
                SummaryHelper.AppendCounters(ref sb_stastic, statistic);
                SummaryHelper.AppendSoftbin(ref sb_sb, dataAcquire, statistic);
                SummaryHelper.AppendHardbin(ref sb_hb, dataAcquire, statistic);

                exporter?.GenerateReport(sb_basic.Append(sb_stastic).Append(sb_sb).Append(sb_hb).ToString());

                var sbNames = dataAcquire.GetSBinInfo();
                //输出statistic中HardBin信息到csv文件中
                logExporter.AppendLine("SoftBin,Name,P/F,Count,Ratio");
                logExporter.AppendLine(
                    string.Join("\n", statistic.SoftBin.Select(kv => $"{kv.Key},{sbNames[kv.Key].Item1},{sbNames[kv.Key].Item2},{kv.Value},{(kv.Value * 100.0 / statistic.TotalCnt)}%"))
                );

            }

            Console.WriteLine("\nFile Summary log done");

            return true;
        }
        
        private static bool checkDataAbnormal(ItemInfo info, NormalityDeviationResult result, ItemStatistic itemStatistic_raw, ItemStatistic itemStatistic_pass, ItemRuleConfig itemRule)
        {
            //先判断是否有上下限，如果没有上下限则不进行正态性检验，直接认为数据正常
            if (info.HiLimit == null && info.LoLimit == null)
            {
                return true;
            }

            //判断是否良率符合预期, 不符合直接返回不正常
            if (itemRule != null)
            {
                double yield = itemStatistic_raw.PassRate;
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
                if(itemRule.SigmaLimit_High != null && itemRule.SigmaLimit_High <= itemStatistic_pass.Sigma)
                {
                    return false;
                }
                if (itemRule.SigmaLimit_Low != null && itemRule.SigmaLimit_Low >= itemStatistic_pass.Sigma)
                {
                    return false;
                }
                if(itemRule.CpkLimit_High != null && !double.IsNaN(itemStatistic_pass.Cpk) && itemRule.CpkLimit_High <= itemStatistic_pass.Cpk)
                {
                    return false;
                }
                if (itemRule.CpkLimit_Low != null && !double.IsNaN(itemStatistic_pass.Cpk) && itemRule.CpkLimit_Low >= itemStatistic_pass.Cpk)
                {
                    return false;
                }
                if(itemRule.MeanLimit_High != null && itemRule.MeanLimit_High <= itemStatistic_pass.MeanValue)
                {
                    return false;
                }
                if (itemRule.MeanLimit_Low != null && itemRule.MeanLimit_Low >= itemStatistic_pass.MeanValue)
                {
                    return false;
                }
                if(itemRule.SkewnessLimit_High != null && itemRule.SkewnessLimit_High <= itemStatistic_pass.Skewness)
                {
                    return false;
                }
                if (itemRule.SkewnessLimit_Low != null && itemRule.SkewnessLimit_Low >= itemStatistic_pass.Skewness)
                {
                    return false;
                }
                if(itemRule.ExcessKurtosisLimit_High != null && itemRule.ExcessKurtosisLimit_High <= itemStatistic_pass.Kurtosis)
                {
                    return false;
                }
                if (itemRule.ExcessKurtosisLimit_Low != null && itemRule.ExcessKurtosisLimit_Low >= itemStatistic_pass.Kurtosis)
                {
                    return false;
                }
                if (itemRule.ModeCountLimit != null && itemRule.ModeCountLimit < result.ModeCount)
                {
                    return false;
                }
                if (itemRule.OutliersLimit != null && itemRule.OutliersLimit < result.OutlierCount)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 分析测试数据并生成报告
        /// </summary>
        private static bool AnalyzeAndGenerateReport(HtmlExpoter exporter, StringBuilder logExporter, string testId, ItemRuleConfig itemRule, ItemAnalysePriority anaflg, DeviationAnalysisParams analysisParams)
        {
            var info = dataAcquire.GetTestInfo(testId);

            var itemStatistic_raw = dataAcquire.GetFilteredStatistic(filterId_raw, testId);
            var itemStatistic_pass = dataAcquire.GetFilteredStatistic(filterId_pass, testId);
            var data_pass = dataAcquire.GetFilteredItemData(testId, filterId_pass);
            var xs_pass = dataAcquire.GetFilteredPartIndex(filterId_pass);


            if (info.HiLimit == null && info.LoLimit == null && !(anaflg == ItemAnalysePriority.ForceAnalyse))
            {
                Console.WriteLine($"  跳过(无管控) TestID: {testId} - {info.TestText}");
                return true;
            } 
            else if(info.HiLimit != null && info.LoLimit != null && info.HiLimit == info.LoLimit && !(anaflg == ItemAnalysePriority.ForceAnalyse))
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
                var s = new System.Diagnostics.Stopwatch();
                s.Start();
                var anomalyAnalysis = dataAcquire.GetFilteredNormalityDeviationResult(filterId_pass, testId, analysisParams);
                s.Stop();
                //Console.WriteLine("NormalityDeviationDetector:" + s.ElapsedMilliseconds);
                if(anomalyAnalysis == null)
                {
                    Console.WriteLine($"正态性偏离分析失败: 结果为null");
                    if (exporter != null && (anaflg == ItemAnalysePriority.ForceAnalyse))
                    {
                        exporter?.GenerateReport($"测试项目: {testId} - {info.TestText}", "正态性偏离分析失败: 结果为null", TestStatus.Warning, null);
                    }
                    return false;
                }
                if (engMode || (anaflg == ItemAnalysePriority.ForceAnalyse))
                {

                    var maxSiteGap = CalcSiteGap(testId, itemStatistic_pass, filterId_pass, info);

                    var anaRst = checkDataAbnormal(info, anomalyAnalysis, itemStatistic_raw, itemStatistic_pass, itemRule);

                    var outRst = $"{(anaRst ? "Pass" : "Fail")}";
                    if (anaflg == ItemAnalysePriority.Ignore) outRst = "Ignore";

                    var description = $"{testId},{info.TestText},{info.HiLimit},{info.LoLimit},{itemStatistic_raw.ValidCount},{100.0 * itemStatistic_raw.PassRate:F4}%," + 
                                    $"{itemStatistic_pass.MeanValue:F3},{itemStatistic_pass.Sigma:F3},{itemStatistic_pass.Cpk:F3}," +
                                    $"{itemStatistic_pass.Skewness:F3},{itemStatistic_pass.Kurtosis:F3}," +
                                    $"{anomalyAnalysis.ModeCount}," +
                                    $"{anomalyAnalysis.OutlierCount}," +
                                    $"{(engMode ? $"{itemStatistic_pass.MedianValue:F3},{anomalyAnalysis.MAD_L:F3},{anomalyAnalysis.MAD_R:F3},{maxSiteGap*100.0:F2}%,{outRst}" : string.Empty)}";
                    logExporter.AppendLine(description);

                    if (exporter == null) return true;

                    if ((!anaRst && anaflg != ItemAnalysePriority.Ignore) || (anaflg == ItemAnalysePriority.ForceAnalyse))
                    {
                        var table = $"HiLimit,LoLimit,数据量,良率,平均值,标准差,CPK,偏度,峰度,密度峰,离群点,{(engMode ? "Median,MAD_L,MAD_R,SiteGap,结果" : string.Empty)}\n" +
                                    $"{info.HiLimit},{info.LoLimit},{itemStatistic_raw.ValidCount},{100.0 * itemStatistic_raw.PassRate:F4}%," +
                                    $"{itemStatistic_pass.MeanValue:F3},{itemStatistic_pass.Sigma:F3},{itemStatistic_pass.Cpk:F3}," +
                                    $"{itemStatistic_pass.Skewness:F3},{itemStatistic_pass.Kurtosis:F3}," +
                                    $"{anomalyAnalysis.ModeCount}," +
                                    $"{anomalyAnalysis.OutlierCount}," +
                                    $"{(engMode ? $"{itemStatistic_pass.MedianValue:F3},{anomalyAnalysis.MAD_L:F3},{anomalyAnalysis.MAD_R:F3},{maxSiteGap * 100.0:F2}%,{(anaRst ? "Pass" : "Fail")}" : string.Empty)}";

                        var status = anaRst ? TestStatus.Pass : TestStatus.Warning;
                        s.Restart();
                        var chartImages = GenerateCharts(testId, anomalyAnalysis);
                        exporter?.GenerateReport($"测试项目: {testId} - {info.TestText}", table, status, chartImages);
                        s.Stop();
                        //Console.WriteLine($"AddAnalysisSlide: {s.ElapsedMilliseconds} ms");
                    }
                }

                return true;
            } catch (Exception ex)
            {
                Console.WriteLine($"处理失败: {ex.Message}");
                return false;
            }
        }

        private static float CalcSiteGap(string testId, ItemStatistic itemStatistic_pass, int filterId_pass, ItemInfo info)
        {
            List<float> siteMedianGapPercentRatio = new List<float>();

            var usl = info.HiLimit ?? itemStatistic_pass.MeanValue + 6 * itemStatistic_pass.Sigma;
            var lsl = info.LoLimit ?? itemStatistic_pass.MeanValue - 6 * itemStatistic_pass.Sigma;
            var bw = (usl - lsl);

            foreach (var site in dataAcquire.GetSites())
            {
                var itemStatistic_site = dataAcquire.GetFilteredStatisticBySite(filterId_pass, testId, site);
                if (itemStatistic_site.ValidCount > 4)
                {
                    siteMedianGapPercentRatio.Add(Math.Abs((itemStatistic_site.MedianValue - itemStatistic_pass.MedianValue) / bw));
                } else
                {
                    siteMedianGapPercentRatio.Add(0);
                }
            }
            var maxSiteGap = siteMedianGapPercentRatio.Max();
            return maxSiteGap;
        }

        /// <summary>
        /// 生成6个图表
        /// </summary>
        private static List<BitMap> GenerateCharts(string testId, NormalityDeviationResult anomalyAnalysis)
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

                if (engMode)
                {
                    var data_raw = dataAcquire.GetFilteredItemData(testId, filterId_raw);
                    var xs_raw = dataAcquire.GetFilteredPartIndex(filterId_raw);
                    var itemStatistic_raw = dataAcquire.GetFilteredStatistic(filterId_raw, testId);
                    float min_raw = info.LoLimit != null ? info.LoLimit.Value : itemStatistic_raw.MeanValue - 6 * itemStatistic_raw.Sigma;
                    float max_raw = info.HiLimit != null ? info.HiLimit.Value : itemStatistic_raw.MeanValue + 6 * itemStatistic_raw.Sigma;

                    var boxPara_pass = new BoxPlotPara(
                                    itemStatistic_pass.MedianValue - (float)(anomalyAnalysis.MadOutlierThRatio_Left * ConsistencyFactor * anomalyAnalysis.MAD_L),
                                    itemStatistic_pass.MedianValue - ConsistencyFactor * anomalyAnalysis.MAD_L,
                                    itemStatistic_pass.MedianValue,
                                    itemStatistic_pass.MedianValue + ConsistencyFactor * anomalyAnalysis.MAD_R,
                                    itemStatistic_pass.MedianValue + (float)(anomalyAnalysis.MadOutlierThRatio_Right * ConsistencyFactor * anomalyAnalysis.MAD_R),
                                    0);

                    charts.Add(new BitMap(ChartGenerator.GenerateComparisonTrendChart(data_raw, xs_raw, data_pass, xs_pass, boxPara_pass, chartTitle, min_pass, max_pass), testId, chartTitle));


                    charts.Add(new BitMap(ChartGenerator.GenerateHistogram(data_raw, boxPara_pass, info.LoLimit, info.HiLimit, $"Raw: {chartTitle}", min_raw, max_raw), testId, chartTitle));

                    var sites = dataAcquire.GetSites();
                    List<BoxPlotPara> boxParas_bySite = new List<BoxPlotPara>();
                    for (int i = 0; i < sites.Length; i++)
                    {
                        var site = sites[i];
                        var (median_site, madLeft_site, madRight_site, siteCount) = dataAcquire.GetFilteredMAD_BySite(filterId_pass, testId, site);
                        var boxPara_site = new BoxPlotPara(
                                        median_site - (float)(anomalyAnalysis.MadOutlierThRatio_Left * ConsistencyFactor * madLeft_site),
                                        median_site - ConsistencyFactor * madLeft_site,
                                        median_site,
                                        median_site + ConsistencyFactor * madRight_site,
                                        median_site + (float)(anomalyAnalysis.MadOutlierThRatio_Right * ConsistencyFactor * madRight_site),
                                        siteCount * sites.Length * 0.9 / itemStatistic_pass.ValidCount);
                        boxParas_bySite.Add(boxPara_site);
                    }
                    charts.Add(new BitMap(ChartGenerator.GenerateBySiteBoxPlot(boxParas_bySite, info.LoLimit, info.HiLimit, chartTitle, min_pass, max_pass), testId, chartTitle));

                    charts.Add(new BitMap(ChartGenerator.GenerateHistogram(data_pass, boxPara_pass, info.LoLimit, info.HiLimit, $"Pass: {chartTitle}", min_pass, max_pass), testId, chartTitle));


                } else
                {
                    var boxPara_pass = new BoxPlotPara(
                                    itemStatistic_pass.MedianValue - (float)(anomalyAnalysis.MadOutlierThRatio_Left * ConsistencyFactor * anomalyAnalysis.MAD_L),
                                    itemStatistic_pass.MedianValue - ConsistencyFactor * anomalyAnalysis.MAD_L,
                                    itemStatistic_pass.MedianValue,
                                    itemStatistic_pass.MedianValue + ConsistencyFactor * anomalyAnalysis.MAD_R,
                                    itemStatistic_pass.MedianValue + (float)(anomalyAnalysis.MadOutlierThRatio_Right * ConsistencyFactor * anomalyAnalysis.MAD_R),
                                    0);

                    charts.Add(new BitMap(ChartGenerator.GenerateTrendChart(data_pass, xs_pass, boxPara_pass, chartTitle, min_pass, max_pass), testId, chartTitle));

                    charts.Add(new BitMap(ChartGenerator.GenerateHistogram(data_pass, boxPara_pass, info.LoLimit, info.HiLimit, chartTitle, min_pass, max_pass), testId, chartTitle));



                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ⚠ 图表生成失败: {ex.Message}");
            }

            return charts;
        }

    }
}
