using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using DataContainer;
using SillyMonkey.Core;
using System.Text;

namespace UI_DataList.ViewModels {
    public class DataSummaryViewModel : BindableBase {
        IEventAggregator _ea;

        string _summary;
        public string Summary {
            get { return _summary; }
            private set { SetProperty(ref _summary, value); } }

        public DataSummaryViewModel(IEventAggregator ea) {
            _ea = ea;
            Summary = "";

            _ea.GetEvent<Event_DataSelected>().Subscribe(UpdateSummary);
            _ea.GetEvent<Event_SubDataSelected>().Subscribe(UpdateSubSummary);
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateSubSummary);

        }

        private void UpdateSubSummary(SubData dataSelected) {
            Summary = GenerateBriefSummary(dataSelected.StdFilePath);
        }

        private void UpdateSummary(string filePath) {
            if(filePath is null) {
                Summary = "";
            } else {
                Summary = GenerateBriefSummary(filePath);
            }
        }

        private string GenerateBriefSummary(string filePath) {
            StringBuilder sb = new StringBuilder();
            var dataAcquire = StdDB.GetDataAcquire(filePath);
            var statistic = dataAcquire.GetPartStatistic();

            AppendBriefBasicInfo(ref sb, dataAcquire);
            AppendBriefCounters(ref sb, statistic);
            return sb.ToString();
        }



        public string GetSummary(IDataAcquire dataAcquire, int filterId) {
            StringBuilder sb = new StringBuilder();

            var statistic = dataAcquire.GetPartStatistic();

            AppendBasicInfo(ref sb, dataAcquire);
            AppendCounters(ref sb, statistic);
            AppendSoftbin(ref sb, dataAcquire, statistic);
            AppendHardbin(ref sb, dataAcquire, statistic);
            //AppendItems();

            return sb.ToString();
        }

        private void AppendBriefBasicInfo(ref StringBuilder sb, IDataAcquire dataAcquire) {
            AppendTitle(ref sb, "Basic Info");

            AppendField(ref sb, "Lot Number", dataAcquire.GetBasicInfo("LotId"));
            AppendField(ref sb, "Sub Lot", dataAcquire.GetBasicInfo("SublotId"));
            //AppendField(ref sb, "Date Code", dataAcquire.GetBasicInfo("DateCode"));

            AppendField(ref sb, "Setup Time", dataAcquire.GetBasicInfo("SetupTime"));
            AppendField(ref sb, "Start Time", dataAcquire.GetBasicInfo("StartTime"));
            AppendField(ref sb, "Finish Time", dataAcquire.GetBasicInfo("FinishTime"));

            AppendLine(ref sb, "");
        }
        private void AppendBasicInfo(ref StringBuilder sb, IDataAcquire dataAcquire) {
            AppendTitle(ref sb, "Basic Info");

            AppendField(ref sb, "File Path", dataAcquire.FilePath);
            AppendField(ref sb, "File Name", dataAcquire.FileName);
            AppendField(ref sb, "Lot Number", dataAcquire.GetBasicInfo("LotId"));
            AppendField(ref sb, "Sub Lot", dataAcquire.GetBasicInfo("SublotId"));
            AppendField(ref sb, "Date Code", dataAcquire.GetBasicInfo("DateCode"));

            AppendField(ref sb, "Setup Time", dataAcquire.GetBasicInfo("SetupTime"));
            AppendField(ref sb, "Start Time", dataAcquire.GetBasicInfo("StartTime"));
            AppendField(ref sb, "Finish Time", dataAcquire.GetBasicInfo("FinishTime"));
            AppendField(ref sb, "Temperature", dataAcquire.GetBasicInfo("TestTemperature"));

            AppendField(ref sb, "Node Name", dataAcquire.GetBasicInfo("NodeName"), true);
            AppendField(ref sb, "Tester Type", dataAcquire.GetBasicInfo("TesterType"), true);
            AppendField(ref sb, "Part Type", dataAcquire.GetBasicInfo("PartType"), true);

            AppendField(ref sb, "Job Name", dataAcquire.GetBasicInfo("JobName"), true);
            AppendField(ref sb, "Job Rev", dataAcquire.GetBasicInfo("JobRevision"), true);
            AppendField(ref sb, "Exec Type", dataAcquire.GetBasicInfo("ExecType"), true);
            AppendField(ref sb, "Exec Ver", dataAcquire.GetBasicInfo("ExecVersion"), true);
            AppendField(ref sb, "Design Rev", dataAcquire.GetBasicInfo("DesignRevision"), true);
            AppendField(ref sb, "Spec Name", dataAcquire.GetBasicInfo("SpecificationName"), true);
            AppendField(ref sb, "Spec Ver", dataAcquire.GetBasicInfo("SpecificationVersion"), true);

            AppendField(ref sb, "Test Code", dataAcquire.GetBasicInfo("TestCode"), true);
            AppendField(ref sb, "ReTest Code", dataAcquire.GetBasicInfo("ReTestCode"), true);
            AppendField(ref sb, "Test Mode", dataAcquire.GetBasicInfo("TestModeCode"), true);
            AppendField(ref sb, "CMD Mode", dataAcquire.GetBasicInfo("CommandModeCode"), true);
            AppendField(ref sb, "Prot Code", dataAcquire.GetBasicInfo("ProtectionCode"), true);

            AppendField(ref sb, "Station ID", dataAcquire.GetBasicInfo("StationNumber"), true);
            AppendField(ref sb, "Flow ID", dataAcquire.GetBasicInfo("FlowID"), true);
            AppendField(ref sb, "Setup ID", dataAcquire.GetBasicInfo("SetupID"), true);
            AppendField(ref sb, "Rom  ID", dataAcquire.GetBasicInfo("RomID"), true);
            AppendField(ref sb, "Aux Data", dataAcquire.GetBasicInfo("AuxiliaryDataFile"), true);
            AppendField(ref sb, "Facility Id", dataAcquire.GetBasicInfo("FacilityId"), true);
            AppendField(ref sb, "Floor ID", dataAcquire.GetBasicInfo("FloorID"), true);
            AppendField(ref sb, "Process ID", dataAcquire.GetBasicInfo("ProcessID"), true);
            AppendField(ref sb, "OP  Freq", dataAcquire.GetBasicInfo("OperationFreq"), true);
            AppendField(ref sb, "Engineering", dataAcquire.GetBasicInfo("EngineeringID"), true);

            AppendField(ref sb, "Operator", dataAcquire.GetBasicInfo("Operator"), true);
            AppendField(ref sb, "Supervisor", dataAcquire.GetBasicInfo("SupervisorID"), true);

            AppendField(ref sb, "PackageType", dataAcquire.GetBasicInfo("PackageType"), true);
            AppendField(ref sb, "Family ID", dataAcquire.GetBasicInfo("FamilyId"), true);
            AppendField(ref sb, "Serial ID", dataAcquire.GetBasicInfo("SerialNumber"), true);

            AppendField(ref sb, "Burn-In", dataAcquire.GetBasicInfo("BurnInTime"), true);
            AppendField(ref sb, "User Text", dataAcquire.GetBasicInfo("UserText"), true);

            AppendLine(ref sb, "");
        }
        private void AppendBriefCounters(ref StringBuilder sb, PartStatistic partStatistic) {
            AppendTitle(ref sb, "Qty Statistic");

            AppendQtyField(ref sb, "Total QTY", partStatistic.TotalCnt);
            AppendQtyField(ref sb, "Pass QTY", partStatistic.PassCnt, partStatistic.TotalCnt);

            AppendQtyField(ref sb, "Fail QTY", partStatistic.FailCnt, partStatistic.TotalCnt);
            AppendQtyField(ref sb, "Abort QTY", partStatistic.AbortCnt, partStatistic.TotalCnt);
            AppendQtyField(ref sb, "Null QTY", partStatistic.NullCnt, partStatistic.TotalCnt);
            AppendLine(ref sb, "");
            AppendQtyField(ref sb, "Fresh QTY", partStatistic.FreshCnt, partStatistic.TotalCnt);
            AppendQtyField(ref sb, "Retest QTY", partStatistic.RtByIdCnt, partStatistic.TotalCnt);

            AppendLine(ref sb, "");
        }


        private void AppendCounters(ref StringBuilder sb, PartStatistic partStatistic) {
            AppendTitle(ref sb, "Qty Statistic");


            var total = partStatistic.SiteCnt;
            var totalNumable = from r in partStatistic.SiteCnt
                               orderby r.Key
                               select r.Value;

            var pass = from r in partStatistic.PassCntBySite
                       orderby r.Key
                       select new Tuple<int, int>(r.Value, total[r.Key]);
            var fail = from r in partStatistic.FailCntBySite
                       orderby r.Key
                       select new Tuple<int, int>(r.Value, total[r.Key]);
            var abort = from r in partStatistic.AbortCntBySite
                       orderby r.Key
                       select new Tuple<int, int>(r.Value, total[r.Key]);
            var empty = from r in partStatistic.NullCntBySite
                       orderby r.Key
                       select new Tuple<int, int>(r.Value, total[r.Key]);
            var fresh = from r in partStatistic.FreshCntBySite
                       orderby r.Key
                       select new Tuple<int, int>(r.Value, total[r.Key]);
            var rt = from r in partStatistic.RtByIdCntBySite
                       orderby r.Key
                       select new Tuple<int, int>(r.Value, total[r.Key]);

            AppendQtyField(ref sb, "Site NO", "All", partStatistic.SiteCnt.Keys);
            AppendQtyField(ref sb, "Total QTY", partStatistic.TotalCnt, totalNumable);
            AppendQtyField(ref sb, "Pass QTY", partStatistic.PassCnt, partStatistic.TotalCnt, pass);

            AppendQtyField(ref sb, "Fail QTY", partStatistic.FailCnt, partStatistic.TotalCnt, fail);
            AppendQtyField(ref sb, "Abort QTY", partStatistic.AbortCnt, partStatistic.TotalCnt, abort);
            AppendQtyField(ref sb, "Null QTY", partStatistic.NullCnt, partStatistic.TotalCnt, empty);
            AppendLine(ref sb, "");
            AppendQtyField(ref sb, "Fresh QTY", partStatistic.FreshCnt, partStatistic.TotalCnt, fresh);
            AppendQtyField(ref sb, "Retest QTY", partStatistic.RtByIdCnt, partStatistic.TotalCnt, rt);

            AppendLine(ref sb, "");
        }

        private void AppendQtyField(ref StringBuilder sb, string fieldName, int fieldCnt, int totalCnt) {
            string fmStr = $"{fieldCnt,7}|{((double)fieldCnt * 100 / (totalCnt == 0 ? 1 : totalCnt)).ToString("f2"),6}%";
            AppendField(ref sb, fieldName, fmStr);
        }

        private void AppendQtyField(ref StringBuilder sb, string fieldName, int fieldCnt, int totalCnt, IEnumerable<Tuple<int, int>> sitesData) {
            string fmStr = $"{fieldCnt,7}|{((double)fieldCnt * 100 / (totalCnt==0? 1: totalCnt)).ToString("f2"),6}%";
            foreach (var v in sitesData) {
                fmStr += $"{v.Item1,7}|{((double)v.Item1 * 100 / (v.Item2==0? 1: v.Item2)).ToString("f2"),6}%";
            }

            AppendField(ref sb, fieldName, fmStr);
        }
        private void AppendQtyField(ref StringBuilder sb, string fieldName, int allData, IEnumerable<int> sitesData) {
            string fmStr = $"{allData,15}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(ref sb, fieldName, fmStr);
        }
        private void AppendQtyField(ref StringBuilder sb, string fieldName, string allData, IEnumerable<byte> sitesData) {
            string fmStr = $"{allData,15}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(ref sb, fieldName, fmStr);
        }
        private void AppendQtyField(ref StringBuilder sb, string fieldName, int allData) {
            string fmStr = $"{allData,15}";
            AppendField(ref sb, fieldName, fmStr);
        }

        private void AppendSoftbin(ref StringBuilder sb, IDataAcquire dataAcquire, PartStatistic partStatistic) {
            AppendTitle(ref sb, "Soft Bin Statistic");
            var total = partStatistic.SiteCnt;

            var siteSb = from r in partStatistic.SoftBinBySite
                         orderby r.Key
                         let t = new Tuple<Dictionary<ushort, int>, int>(r.Value, total[r.Key])
                         select t;

            var sbin = from r in partStatistic.SoftBin
                       orderby r.Key
                       select r;

            var sbNames = dataAcquire.GetSBinInfo();

            AppendBinField(ref sb, "Site NO", "All", partStatistic.SiteCnt.Keys);
            foreach (var b in sbin) {
                AppendBinField(ref sb, new Tuple<KeyValuePair<ushort, int>, int>(b, partStatistic.TotalCnt), $"{sbNames[b.Key].Item2}:{sbNames[b.Key].Item1}", siteSb);
            }
            AppendLine(ref sb, "");
        }

        private void AppendHardbin(ref StringBuilder sb, IDataAcquire dataAcquire, PartStatistic partStatistic) {
            AppendTitle(ref sb, "Hard Bin Statistic");

            var total = partStatistic.SiteCnt;

            var siteHb = from r in partStatistic.HardBinBySite
                         orderby r.Key
                         let t = new Tuple<Dictionary<ushort, int>, int>(r.Value, total[r.Key])
                         select t;

            var hb = from r in partStatistic.HardBin
                     orderby r.Key
                     select r;

            var hbNames = dataAcquire.GetHBinInfo();

            AppendBinField(ref sb, "Site NO", "All", partStatistic.SiteCnt.Keys);
            foreach (var b in hb) {
                AppendBinField(ref sb, new Tuple<KeyValuePair<ushort, int>, int>(b, partStatistic.TotalCnt), $"{hbNames[b.Key].Item2}:{hbNames[b.Key].Item1}", siteHb);
            }
            AppendLine(ref sb, "");
        }

        private void AppendBinField(ref StringBuilder sb, Tuple<KeyValuePair<ushort, int>, int> bin, string binName, IEnumerable<Tuple<Dictionary<ushort, int>, int>> siteBin) {
            string fmStr = $"{binName,-20}{bin.Item1.Value,7}|{((double)bin.Item1.Value * 100 / bin.Item2).ToString("f2"),6}%";
            foreach (var v in siteBin) {
                if (v.Item1.ContainsKey(bin.Item1.Key))
                    fmStr += $"{v.Item1[bin.Item1.Key],7}|{((double)v.Item1[bin.Item1.Key] * 100 / v.Item2).ToString("f2"),6}%";
                else
                    fmStr += $"{0,7}|{((double)0 * 100 / v.Item2).ToString("f2"),6}%";

            }

            AppendField(ref sb, bin.Item1.Key.ToString(), fmStr);
        }

        private void AppendBinField(ref StringBuilder sb, string fieldName, string allData, IEnumerable<byte> sitesData) {
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

        private void AppendField(ref StringBuilder sb, string fieldName, string content, bool skipIfNull = false) {
            if (skipIfNull) {
                if (content == null || content.Trim().Length == 0) return;
            }

            sb.Append($"{fieldName + ":",-16}{content}\r\n");

            //Run runField = new Run() { Text = $"{fieldName+":", -16}", Foreground = new SolidColorBrush(Colors.Black), FontWeight = FontWeight.FromOpenTypeWeight(700) };
            //Run runContent = new Run() { Text = content + "\r\n", Foreground = new SolidColorBrush(Colors.Black) };
            //_paragraph.Inlines.Add(runField);
            //_paragraph.Inlines.Add(runContent);

        }

        private void AppendTitle(ref StringBuilder sb, string title) {

            sb.Append($"{title}\r\n");

            //Run run = new Run() { Text = title + "\r\n", Foreground = new SolidColorBrush(Colors.Black), FontWeight= FontWeight.FromOpenTypeWeight(900), FontSize=20 };
            //_paragraph.Inlines.Add(run);

        }

        private void Append(ref StringBuilder sb, string text) {
            sb.Append(text);
            //Run run = new Run() { Text = text};
            //_paragraph.Inlines.Add(run);
        }

        private void AppendLine(ref StringBuilder sb, string text) {
            sb.AppendLine(text);
            //Run run = new Run() { Text = text + "\r\n" };
            //_paragraph.Inlines.Add(run);
        }

    }
}
