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
        public bool SlidesExport { get; set; } = false;

        [JsonProperty]
        public List<DataFileConfig> DataFiles { get; set; } = new List<DataFileConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PassBin { get; set; } = 1;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<ushort, BinRuleConfig> SoftBinRule { get; set; } = new Dictionary<ushort, BinRuleConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<ushort, BinRuleConfig> HardBinRule { get; set; } = new Dictionary<ushort, BinRuleConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ItemRuleConfig GeneralRule { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ItemRuleConfig> TargetItemsAndRule { get; set; } = new Dictionary<string, ItemRuleConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OutputFolder { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SlidesTemplatePath { get; set; }
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
        public double? JarqueBeraLimit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? ModeCountLimit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? ClustersLimit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? OutliersLimit { get; set; }

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
