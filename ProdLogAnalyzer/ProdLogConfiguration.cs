using System.Collections.Generic;
using DataContainer;
using Newtonsoft.Json;

namespace ProdLogAnalyzer
{
    [JsonObject]
    public class ProdLogConfiguration
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty]
        public bool EngMode { get; set; } = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool ReportExport { get; set; } = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LotInfoRegex { get; set; } = string.Empty;

        [JsonProperty]
        public List<DataFileConfig> DataFiles { get; set; } = new List<DataFileConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PassBin { get; set; } = 1;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<ushort, BinRuleConfig> SoftBinRule { get; set; } = new Dictionary<ushort, BinRuleConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<ushort, BinRuleConfig> HardBinRule { get; set; } = new Dictionary<ushort, BinRuleConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> IgnoredItemsByTestId { get; set; } = new List<string>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> IgnoredItemsByTestTextRegex { get; set; } = new List<string>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ItemRuleConfig GeneralRule { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ItemRuleConfig> TargetItemsAndRule { get; set; } = new Dictionary<string, ItemRuleConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OutputFolder { get; set; }
    }

    [JsonObject]
    public class DataFileConfig
    {
        [JsonProperty]
        public string Path { get; set; }

        // 如果 FilterSetup 没有公共无参构造函数，反序列化时可能需要自定义 JsonConverter
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FilterSetup Filter { get; set; }
    }

    [JsonObject]
    public class ItemRuleConfig
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? YieldLimit_High { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? YieldLimit_Low { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? SigmaLimit_High { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? SigmaLimit_Low { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? CpkLimit_High { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? CpkLimit_Low { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? MeanLimit_High { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? MeanLimit_Low { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? SkewnessLimit_High { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? SkewnessLimit_Low { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? ExcessKurtosisLimit_High { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? ExcessKurtosisLimit_Low { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? ModeCountLimit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? OutliersLimit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Para_MAD_Threshold_Left { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Para_MAD_Threshold_Right { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Para_MAD_Threshold_HalfLimit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Para_BandwidthRatio { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Para_KernelSampleCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Para_PeakProminenceRatio { get; set; }

        public ItemRuleConfig(ItemRuleConfig config)
        {
            if (config != null)
            {
                YieldLimit_High = config.YieldLimit_High;
                YieldLimit_Low = config.YieldLimit_Low;
                SigmaLimit_High = config.SigmaLimit_High;
                SigmaLimit_Low = config.SigmaLimit_Low;
                CpkLimit_High = config.CpkLimit_High;
                CpkLimit_Low = config.CpkLimit_Low;
                MeanLimit_High = config.MeanLimit_High;
                MeanLimit_Low = config.MeanLimit_Low;
                SkewnessLimit_High = config.SkewnessLimit_High;
                SkewnessLimit_Low = config.SkewnessLimit_Low;
                ExcessKurtosisLimit_High = config.ExcessKurtosisLimit_High;
                ExcessKurtosisLimit_Low = config.ExcessKurtosisLimit_Low;
                ModeCountLimit = config.ModeCountLimit;
                OutliersLimit = config.OutliersLimit;
                Para_MAD_Threshold_Left = config.Para_MAD_Threshold_Left;
                Para_MAD_Threshold_Right = config.Para_MAD_Threshold_Right;
                Para_MAD_Threshold_HalfLimit = config.Para_MAD_Threshold_HalfLimit;
                Para_BandwidthRatio = config.Para_BandwidthRatio;
                Para_KernelSampleCount = config.Para_KernelSampleCount;
                Para_PeakProminenceRatio = config.Para_PeakProminenceRatio;
            }
        }
    }

    [JsonObject]
    public class BinRuleConfig
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? BinLimit_High { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? BinLimit_Low { get; set; }

    }

}
