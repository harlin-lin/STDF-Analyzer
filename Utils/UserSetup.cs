using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils {
    public enum ChartAxisType {
        Sigma,
        MinMax,
        Limit,
        User
    }

    public enum UidType {
        TestNumber,
        TestName,
        TestNumberAndTestName
    }

    public enum SigmaRangeType {
        Sigma6,
        Sigma5,
        Sigma4,
        Sigma3,
        Sigma2,
        Sigma1
    }

    public class UserSetup {
        public UidType UidMode = UidType.TestNumber;

        //Histogram
        public ChartAxisType SetupHistogramChartAxis = ChartAxisType.Sigma;
        public SigmaRangeType SetupHistogramChartAxisSigmaRange = SigmaRangeType.Sigma6;
        public bool SetupHistogramEnableOutlierFilter = true;
        public SigmaRangeType SetupHistogramOutlierFilterRange = SigmaRangeType.Sigma6;
        public bool SetupHistogramEnableLimitLine = true;
        public bool SetupHistogramEnableSigma6Line = true;
        public bool SetupHistogramEnableSigma3Line = false;
        public bool SetupHistogramEnableMinMaxLine = false;
        public bool SetupHistogramEnableMeanLine = false;
        public bool SetupHistogramEnableMedianLine = false;

        //Trend
        public ChartAxisType SetupTrendChartAxis = ChartAxisType.Sigma;
        public SigmaRangeType SetupTrendChartAxisSigmaRange = SigmaRangeType.Sigma6;
        public bool SetupTrendEnableOutlierFilter = true;
        public SigmaRangeType SetupTrendOutlierFilterRange = SigmaRangeType.Sigma6;
        public bool SetupTrendEnableLimitLine = true;
        public bool SetupTrendEnableSigma6Line = true;
        public bool SetupTrendEnableSigma3Line = false;
        public bool SetupTrendEnableMinMaxLine = false;
        public bool SetupTrendEnableMeanLine = false;
        public bool SetupTrendEnableMedianLine = false;

        //CorrHistogram
        public ChartAxisType SetupCorrHistogramChartAxis = ChartAxisType.Sigma;
        public SigmaRangeType SetupCorrHistogramChartAxisSigmaRange = SigmaRangeType.Sigma6;
        public bool SetupCorrHistogramEnableOutlierFilter = true;
        public SigmaRangeType SetupCorrHistogramOutlierFilterRange = SigmaRangeType.Sigma6;


        //ItemCorr
        public bool SetupItemCorrEnableOutlierFilter = true;
        public SigmaRangeType SetupItemCorrOutlierFilterRange = SigmaRangeType.Sigma6;


    }
}
