using DataInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileReader {
    public static class Summary {

        public static string GetSummary(IDataAcquire dataAcquire, int? filterId) {
            IChipSummary summary;
            IOrderedEnumerable<KeyValuePair<byte, IChipSummary>> ss;

            if (filterId.HasValue) {
                summary = dataAcquire.GetFilteredChipSummary(filterId.Value);
                ss = dataAcquire.GetFilteredChipSummaryBySite(filterId.Value).OrderBy(x => x.Key);
            } else {
                summary = dataAcquire.GetChipSummary();
                ss = dataAcquire.GetChipSummaryBySite().OrderBy(x => x.Key);
            }


            IEnumerable<IChipSummary>  sitesSummary = from r in ss
                            select r.Value;
            IEnumerable<string>  sitesNO = from r in ss
                        let s = "Site:" + r.Key
                        select s;

            StringBuilder sb = new StringBuilder();

            AppendBasicInfo(ref sb, dataAcquire);
            AppendCounters(ref sb, summary, sitesSummary, sitesNO);
            AppendSoftbin(ref sb, dataAcquire, summary, sitesSummary, sitesNO);
            AppendHardbin(ref sb, dataAcquire, summary, sitesSummary, sitesNO);
            //AppendItems();

            return sb.ToString();
        }

        private static void AppendBasicInfo(ref StringBuilder sb, IDataAcquire dataAcquire) {
            AppendTitle(ref sb, "Basic Info");

            AppendField(ref sb, "File Path", dataAcquire.FilePath);
            AppendField(ref sb, "File Name", dataAcquire.FileName);
            AppendField(ref sb, "Lot Number", dataAcquire.BasicInfo.LotId);
            AppendField(ref sb, "Sub Lot", dataAcquire.BasicInfo.SublotId);
            AppendField(ref sb, "Date Code", dataAcquire.BasicInfo.DateCode);

            AppendField(ref sb, "Setup Time", dataAcquire.BasicInfo.SetupTime.ToString());
            AppendField(ref sb, "Start Time", dataAcquire.BasicInfo.StartTime.ToString());
            AppendField(ref sb, "Finish Time", dataAcquire.BasicInfo.FinishTime.ToString());
            AppendField(ref sb, "Temperature", dataAcquire.BasicInfo.TestTemperature);

            AppendField(ref sb, "Node Name", dataAcquire.BasicInfo.NodeName, true);
            AppendField(ref sb, "Tester Type", dataAcquire.BasicInfo.TesterType, true);
            AppendField(ref sb, "Part Type", dataAcquire.BasicInfo.PartType, true);

            AppendField(ref sb, "Job Name", dataAcquire.BasicInfo.JobName, true);
            AppendField(ref sb, "Job Rev", dataAcquire.BasicInfo.JobRevision, true);
            AppendField(ref sb, "Exec Type", dataAcquire.BasicInfo.ExecType, true);
            AppendField(ref sb, "Exec Ver", dataAcquire.BasicInfo.ExecVersion, true);
            AppendField(ref sb, "Design Rev", dataAcquire.BasicInfo.DesignRevision, true);
            AppendField(ref sb, "Spec Name", dataAcquire.BasicInfo.SpecificationName, true);
            AppendField(ref sb, "Spec Ver", dataAcquire.BasicInfo.SpecificationVersion, true);

            AppendField(ref sb, "Test Code", dataAcquire.BasicInfo.TestCode, true);
            AppendField(ref sb, "ReTest Code", dataAcquire.BasicInfo.ReTestCode, true);
            AppendField(ref sb, "Test Mode", dataAcquire.BasicInfo.TestModeCode, true);
            AppendField(ref sb, "CMD Mode", dataAcquire.BasicInfo.CommandModeCode, true);
            AppendField(ref sb, "Prot Code", dataAcquire.BasicInfo.ProtectionCode, true);

            AppendField(ref sb, "Station ID", dataAcquire.BasicInfo.StationNumber.ToString(), true);
            AppendField(ref sb, "Flow ID", dataAcquire.BasicInfo.FlowID, true);
            AppendField(ref sb, "Setup ID", dataAcquire.BasicInfo.SetupID, true);
            AppendField(ref sb, "Rom  ID", dataAcquire.BasicInfo.RomID, true);
            AppendField(ref sb, "Aux Data", dataAcquire.BasicInfo.AuxiliaryDataFile, true);
            AppendField(ref sb, "Facility Id", dataAcquire.BasicInfo.FacilityId, true);
            AppendField(ref sb, "Floor ID", dataAcquire.BasicInfo.FloorID, true);
            AppendField(ref sb, "Process ID", dataAcquire.BasicInfo.ProcessID, true);
            AppendField(ref sb, "OP  Freq", dataAcquire.BasicInfo.OperationFreq, true);
            AppendField(ref sb, "Engineering", dataAcquire.BasicInfo.EngineeringID, true);

            AppendField(ref sb, "Operator", dataAcquire.BasicInfo.Operator, true);
            AppendField(ref sb, "Supervisor", dataAcquire.BasicInfo.SupervisorID, true);

            AppendField(ref sb, "PackageType", dataAcquire.BasicInfo.PackageType, true);
            AppendField(ref sb, "Family ID", dataAcquire.BasicInfo.FamilyId, true);
            AppendField(ref sb, "Serial ID", dataAcquire.BasicInfo.SerialNumber, true);

            AppendField(ref sb, "Burn-In", dataAcquire.BasicInfo.BurnInTime.ToString(), true);
            AppendField(ref sb, "User Text", dataAcquire.BasicInfo.UserText, true);

            AppendLine(ref sb, "");
        }

        private static void AppendCounters(ref StringBuilder sb, IChipSummary summary, IEnumerable<IChipSummary> sitesSummary, IEnumerable<string> sitesNO) {
            AppendTitle(ref sb, "Qty Statistic");

            var total = from r in sitesSummary
                        select r.TotalCount;
            var pass = from r in sitesSummary
                       let t = new Tuple<int, int>(r.PassCount, r.TotalCount)
                       select t;

            var fail = from r in sitesSummary
                       let t = new Tuple<int, int>(r.FailCount, r.TotalCount)
                       select t;
            var abort = from r in sitesSummary
                       let t = new Tuple<int, int>(r.AbortCount, r.TotalCount)
                       select t;
            var empty = from r in sitesSummary
                       let t = new Tuple<int, int>(r.NullCount, r.TotalCount)
                       select t;
            var fresh = from r in sitesSummary
                       let t = new Tuple<int, int>(r.FreshCount, r.TotalCount)
                       select t;
            var rt = from r in sitesSummary
                       let t = new Tuple<int, int>(r.RetestCount, r.TotalCount)
                       select t;

            AppendQtyField(ref sb, "Site NO", "All", sitesNO);
            AppendQtyField(ref sb, "Total QTY", summary.TotalCount, total);
            AppendQtyField(ref sb, "Pass QTY", new Tuple<int, int>(summary.PassCount, summary.TotalCount), pass);

            AppendQtyField(ref sb, "Fail QTY", new Tuple<int, int>(summary.FailCount, summary.TotalCount), fail);
            AppendQtyField(ref sb, "Abort QTY", new Tuple<int, int>(summary.AbortCount, summary.TotalCount), abort);
            AppendQtyField(ref sb, "Null QTY", new Tuple<int, int>(summary.NullCount, summary.TotalCount), empty);
            AppendLine(ref sb, "");
            AppendQtyField(ref sb, "Fresh QTY", new Tuple<int, int>(summary.FreshCount, summary.TotalCount), fresh);
            AppendQtyField(ref sb, "Retest QTY", new Tuple<int, int>(summary.RetestCount, summary.TotalCount), rt);

            AppendLine(ref sb, "");
        }

        private static void AppendQtyField(ref StringBuilder sb, string fieldName, Tuple<int,int> allData, IEnumerable<Tuple<int, int>> sitesData) {
            string fmStr = $"{allData.Item1,7}|{((double)allData.Item1 * 100 / allData.Item2).ToString("f2"),6}%";
            foreach(var v in sitesData) {
                fmStr += $"{v.Item1,7}|{((double)v.Item1 * 100 / v.Item2).ToString("f2"),6}%";
            }

            AppendField(ref sb, fieldName, fmStr);
        }
        private static void AppendQtyField(ref StringBuilder sb, string fieldName, int allData, IEnumerable<int> sitesData) {
            string fmStr = $"{allData,15}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(ref sb, fieldName, fmStr);
        }
        private static void AppendQtyField(ref StringBuilder sb, string fieldName, string allData, IEnumerable<string> sitesData) {
            string fmStr = $"{allData,15}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(ref sb, fieldName, fmStr);
        }

        private static void AppendSoftbin(ref StringBuilder sb, IDataAcquire dataAcquire,  IChipSummary summary, IEnumerable<IChipSummary> sitesSummary, IEnumerable<string> sitesNO) {
            AppendTitle(ref sb, "Soft Bin Statistic");

            var siteSb = from r in sitesSummary
                       let t = new Tuple<Dictionary<ushort,int>, int>(r.GetSoftBins(), r.TotalCount)
                       select t;

            var sbin = from r in summary.GetSoftBins()
                     orderby r.Key
                     select r;

            var sbNames = dataAcquire.GetSBinInfo(); 

            AppendBinField(ref sb, "Site NO", "All", sitesNO);
            foreach (var b in sbin) {
                AppendBinField(ref sb, new Tuple<KeyValuePair<ushort, int>, int>(b, summary.TotalCount), $"{sbNames[b.Key].Item2}:{sbNames[b.Key].Item1}", siteSb);
            }
            AppendLine(ref sb, "");
        }

        private static void AppendHardbin(ref StringBuilder sb, IDataAcquire dataAcquire, IChipSummary summary, IEnumerable<IChipSummary> sitesSummary, IEnumerable<string> sitesNO) {
            AppendTitle(ref sb, "Hard Bin Statistic");

            var siteHb = from r in sitesSummary
                         let t = new Tuple<Dictionary<ushort, int>, int>(r.GetHardBins(), r.TotalCount)
                         select t;

            var hb = from r in summary.GetHardBins()
                     orderby r.Key
                     select r;

            var hbNames = dataAcquire.GetHBinInfo();

            AppendBinField(ref sb, "Site NO", "All", sitesNO);
            foreach (var b in hb) {
                AppendBinField(ref sb, new Tuple<KeyValuePair<ushort, int>, int>(b, summary.TotalCount), $"{hbNames[b.Key].Item2}:{hbNames[b.Key].Item1}", siteHb);
            }
            AppendLine(ref sb, "");
        }

        private static void AppendBinField(ref StringBuilder sb, Tuple<KeyValuePair<ushort, int>, int> bin, string binName, IEnumerable<Tuple<Dictionary<ushort, int>, int>> siteBin) {
            string fmStr = $"{binName,-20}{bin.Item1.Value,7}|{((double)bin.Item1.Value * 100 / bin.Item2).ToString("f2"),6}%";
            foreach (var v in siteBin) {
                if (v.Item1.ContainsKey(bin.Item1.Key))
                    fmStr += $"{v.Item1[bin.Item1.Key],7}|{((double)v.Item1[bin.Item1.Key] * 100 / v.Item2).ToString("f2"),6}%";
                else
                    fmStr += $"{0,7}|{((double)0 * 100 / v.Item2).ToString("f2"),6}%";

            }

            AppendField(ref sb, bin.Item1.Key.ToString(), fmStr);
        }

        private static void AppendBinField(ref StringBuilder sb, string fieldName, string allData, IEnumerable<string> sitesData) {
            string fmStr = $"{"Bin Name",-20}{allData,14}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(ref sb, fieldName, fmStr);
        }

        //private void AppendItems() {
        //    AppendTitle("Test Item Statistic");

        //    var ids = dataAcquire.GetTestIDs_Info();

        //    var sts = dataAcquire.GetFilteredStatistic(filterId);

        //    foreach(var v in ids) {

        //    }

        //    AppendLine("");
        //}

        private static void AppendField(ref StringBuilder sb, string fieldName, string content, bool skipIfNull=false) {
            if (skipIfNull) {
                if (content==null || content.Trim().Length == 0) return; 
            }

            sb.Append($"{fieldName + ":",-16}{content}\r\n");

            //Run runField = new Run() { Text = $"{fieldName+":", -16}", Foreground = new SolidColorBrush(Colors.Black), FontWeight = FontWeight.FromOpenTypeWeight(700) };
            //Run runContent = new Run() { Text = content + "\r\n", Foreground = new SolidColorBrush(Colors.Black) };
            //_paragraph.Inlines.Add(runField);
            //_paragraph.Inlines.Add(runContent);

        }

        private static void AppendTitle(ref StringBuilder sb, string title) {

            sb.Append($"{title}\r\n");

            //Run run = new Run() { Text = title + "\r\n", Foreground = new SolidColorBrush(Colors.Black), FontWeight= FontWeight.FromOpenTypeWeight(900), FontSize=20 };
            //_paragraph.Inlines.Add(run);

        }

        private static void Append(ref StringBuilder sb, string text) {
            sb.Append(text);
            //Run run = new Run() { Text = text};
            //_paragraph.Inlines.Add(run);
        }

        private static void AppendLine(ref StringBuilder sb, string text) {
            sb.AppendLine(text);
            //Run run = new Run() { Text = text + "\r\n" };
            //_paragraph.Inlines.Add(run);
        }

    }
}
