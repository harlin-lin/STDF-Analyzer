using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public class ItemInfo {
        public string TestName { get; private set; }
        public float? LoLimit { get; private set; }
        public float? HiLimit { get; private set; }
        public string Unit { get; private set; }
        public sbyte? RstScale { get; private set; }
        public float MeanValue { get; private set; }
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }
        public double Cp { get; private set; }
        public double Cpk { get; private set; }
        public double? Sigma { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }

    }
}
