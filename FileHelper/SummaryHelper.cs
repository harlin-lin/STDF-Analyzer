using DataInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FileHelper {
    public class SummaryHelper {
        private FlowDocument _flowDocument;
        private Paragraph _paragraph;
        private IDataAcquire _dataAcquire;
        private IChipSummary summary;
        private IEnumerable<IChipSummary> sitesSummary;
        private IEnumerable<string> sitesNO;
        private int _filter;

        public SummaryHelper(IDataAcquire dataAcquire, int filter) {
            _flowDocument = new FlowDocument();
            _paragraph = new Paragraph();
            _dataAcquire = dataAcquire;
            _filter = filter;

            summary = _dataAcquire.GetFilteredChipSummary(_filter);
            var ss = _dataAcquire.GetFilteredChipSummaryBySite(_filter).OrderBy(x=>x.Key);
            sitesSummary = from r in ss
                               select r.Value;
            sitesNO = from r in ss
                          let s = "Site:" + r.Key
                          select s;
        }

        public FlowDocument GetSummary() {
            AppendBasicInfo();
            AppendCounters();
            AppendSoftbin();
            AppendHardbin();
            //AppendItems();

            _flowDocument.Blocks.Add(_paragraph);

            return _flowDocument;
        }

        private void AppendBasicInfo() {
            AppendTitle("Basic Info");

            AppendField("File Path", _dataAcquire.FilePath);
            AppendField("File Name", _dataAcquire.FileName);
            AppendField("Lot Number", _dataAcquire.BasicInfo.LotId);
            AppendField("Sub Lot", _dataAcquire.BasicInfo.SublotId);
            AppendField("Date Code", _dataAcquire.BasicInfo.DateCode);

            AppendField("Setup Time", _dataAcquire.BasicInfo.SetupTime.ToString());
            AppendField("Start Time", _dataAcquire.BasicInfo.StartTime.ToString());
            AppendField("Finish Time", _dataAcquire.BasicInfo.FinishTime.ToString());
            AppendField("Temperature", _dataAcquire.BasicInfo.TestTemperature);

            AppendField("Node Name", _dataAcquire.BasicInfo.NodeName, true);
            AppendField("Tester Type", _dataAcquire.BasicInfo.TesterType, true);
            AppendField("Part Type", _dataAcquire.BasicInfo.PartType, true);

            AppendField("Job Name", _dataAcquire.BasicInfo.JobName, true);
            AppendField("Job Rev", _dataAcquire.BasicInfo.JobRevision, true);
            AppendField("Exec Type", _dataAcquire.BasicInfo.ExecType, true);
            AppendField("Exec Ver", _dataAcquire.BasicInfo.ExecVersion, true);
            AppendField("Design Rev", _dataAcquire.BasicInfo.DesignRevision, true);
            AppendField("Spec Name", _dataAcquire.BasicInfo.SpecificationName, true);
            AppendField("Spec Ver", _dataAcquire.BasicInfo.SpecificationVersion, true);

            AppendField("Test Code", _dataAcquire.BasicInfo.TestCode, true);
            AppendField("ReTest Code", _dataAcquire.BasicInfo.ReTestCode, true);
            AppendField("Test Mode", _dataAcquire.BasicInfo.TestModeCode, true);
            AppendField("CMD Mode", _dataAcquire.BasicInfo.CommandModeCode, true);
            AppendField("Prot Code", _dataAcquire.BasicInfo.ProtectionCode, true);

            AppendField("Station ID", _dataAcquire.BasicInfo.StationNumber.ToString(), true);
            AppendField("Flow ID", _dataAcquire.BasicInfo.FlowID, true);
            AppendField("Setup ID", _dataAcquire.BasicInfo.SetupID, true);
            AppendField("Rom  ID", _dataAcquire.BasicInfo.RomID, true);
            AppendField("Aux Data", _dataAcquire.BasicInfo.AuxiliaryDataFile, true);
            AppendField("Facility Id", _dataAcquire.BasicInfo.FacilityId, true);
            AppendField("Floor ID", _dataAcquire.BasicInfo.FloorID, true);
            AppendField("Process ID", _dataAcquire.BasicInfo.ProcessID, true);
            AppendField("OP  Freq", _dataAcquire.BasicInfo.OperationFreq, true);
            AppendField("Engineering", _dataAcquire.BasicInfo.EngineeringID, true);

            AppendField("Operator", _dataAcquire.BasicInfo.Operator, true);
            AppendField("Supervisor", _dataAcquire.BasicInfo.SupervisorID, true);

            AppendField("PackageType", _dataAcquire.BasicInfo.PackageType, true);
            AppendField("Family ID", _dataAcquire.BasicInfo.FamilyId, true);
            AppendField("Serial ID", _dataAcquire.BasicInfo.SerialNumber, true);

            AppendField("Burn-In", _dataAcquire.BasicInfo.BurnInTime.ToString(), true);
            AppendField("User Text", _dataAcquire.BasicInfo.UserText, true);

            AppendLine("");
        }

        private void AppendCounters() {
            AppendTitle("Qty Statistic");

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

            AppendQtyField("Site NO", "All", sitesNO);
            AppendQtyField("Total QTY", summary.TotalCount, total);
            AppendQtyField("Pass QTY", new Tuple<int, int>(summary.PassCount, summary.TotalCount), pass);

            AppendQtyField("Fail QTY", new Tuple<int, int>(summary.FailCount, summary.TotalCount), fail);
            AppendQtyField("Abort QTY", new Tuple<int, int>(summary.AbortCount, summary.TotalCount), abort);
            AppendQtyField("Null QTY", new Tuple<int, int>(summary.NullCount, summary.TotalCount), empty);
            AppendLine("");
            AppendQtyField("Fresh QTY", new Tuple<int, int>(summary.FreshCount, summary.TotalCount), fresh);
            AppendQtyField("Retest QTY", new Tuple<int, int>(summary.RetestCount, summary.TotalCount), rt);

            AppendLine("");
        }

        private void AppendQtyField(string fieldName, Tuple<int,int> allData, IEnumerable<Tuple<int, int>> sitesData) {
            string fmStr = $"{allData.Item1,7}|{((double)allData.Item1 * 100 / allData.Item2).ToString("f2"),6}%";
            foreach(var v in sitesData) {
                fmStr += $"{v.Item1,7}|{((double)v.Item1 * 100 / v.Item2).ToString("f2"),6}%";
            }

            AppendField(fieldName, fmStr);
        }
        private void AppendQtyField(string fieldName, int allData, IEnumerable<int> sitesData) {
            string fmStr = $"{allData,15}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(fieldName, fmStr);
        }
        private void AppendQtyField(string fieldName, string allData, IEnumerable<string> sitesData) {
            string fmStr = $"{allData,15}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(fieldName, fmStr);
        }

        private void AppendSoftbin() {
            AppendTitle("Soft Bin Statistic");

            var siteSb = from r in sitesSummary
                       let t = new Tuple<Dictionary<ushort,int>, int>(r.GetSoftBins(), r.TotalCount)
                       select t;

            var sb = from r in summary.GetSoftBins()
                     orderby r.Key
                     select r;

            var sbNames = _dataAcquire.GetSBinInfo(); 

            AppendBinField("Site NO", "All", sitesNO);
            foreach (var b in sb) {
                AppendBinField(new Tuple<KeyValuePair<ushort, int>, int>(b, summary.TotalCount), $"{sbNames[b.Key].Item2}:{sbNames[b.Key].Item1}", siteSb);
            }
            AppendLine("");
        }

        private void AppendHardbin() {
            AppendTitle("Hard Bin Statistic");

            var siteHb = from r in sitesSummary
                         let t = new Tuple<Dictionary<ushort, int>, int>(r.GetHardBins(), r.TotalCount)
                         select t;

            var hb = from r in summary.GetHardBins()
                     orderby r.Key
                     select r;

            var hbNames = _dataAcquire.GetHBinInfo();

            AppendBinField("Site NO", "All", sitesNO);
            foreach (var b in hb) {
                AppendBinField(new Tuple<KeyValuePair<ushort, int>, int>(b, summary.TotalCount), $"{hbNames[b.Key].Item2}:{hbNames[b.Key].Item1}", siteHb);
            }
            AppendLine("");
        }

        private void AppendBinField(Tuple<KeyValuePair<ushort, int>, int> bin, string binName, IEnumerable<Tuple<Dictionary<ushort, int>, int>> siteBin) {
            string fmStr = $"{binName,-20}{bin.Item1.Value,7}|{((double)bin.Item1.Value * 100 / bin.Item2).ToString("f2"),6}%";
            foreach (var v in siteBin) {
                if (v.Item1.ContainsKey(bin.Item1.Key))
                    fmStr += $"{v.Item1[bin.Item1.Key],7}|{((double)v.Item1[bin.Item1.Key] * 100 / v.Item2).ToString("f2"),6}%";
                else
                    fmStr += $"{0,7}|{((double)0 * 100 / v.Item2).ToString("f2"),6}%";

            }

            AppendField(bin.Item1.Key.ToString(), fmStr);
        }

        private void AppendBinField(string fieldName, string allData, IEnumerable<string> sitesData) {
            string fmStr = $"{"Bin Name",-20}{allData,14}";
            foreach (var v in sitesData) {
                fmStr += $"{v,15}";
            }

            AppendField(fieldName, fmStr);
        }

        //private void AppendItems() {
        //    AppendTitle("Test Item Statistic");

        //    var ids = _dataAcquire.GetTestIDs_Info();

        //    var sts = _dataAcquire.GetFilteredStatistic(_filter);

        //    foreach(var v in ids) {

        //    }

        //    AppendLine("");
        //}

        private void AppendField(string fieldName, string content, bool skipIfNull=false) {
            if (skipIfNull) {
                if (content==null || content.Trim().Length == 0) return; 
            }

            Run runField = new Run() { Text = $"{fieldName+":", -16}", Foreground = new SolidColorBrush(Colors.Black), FontWeight = FontWeight.FromOpenTypeWeight(700) };
            Run runContent = new Run() { Text = content + "\r\n", Foreground = new SolidColorBrush(Colors.Black) };
            _paragraph.Inlines.Add(runField);
            _paragraph.Inlines.Add(runContent);

        }

        private void AppendTitle(string title) {

            Run run = new Run() { Text = title + "\r\n", Foreground = new SolidColorBrush(Colors.Black), FontWeight= FontWeight.FromOpenTypeWeight(900), FontSize=20 };
            _paragraph.Inlines.Add(run);

        }

        private void Append(string text) {
            Run run = new Run() { Text = text};
            _paragraph.Inlines.Add(run);
        }

        private void AppendLine(string text) {
            Run run = new Run() { Text = text + "\r\n" };
            _paragraph.Inlines.Add(run);
        }


        public static string RTF(RichTextBox richTextBox) {
            string rtf = string.Empty;
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream()) {
                textRange.Save(ms, System.Windows.DataFormats.Rtf);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(ms);
                rtf = sr.ReadToEnd();
            }

            return rtf;
        }

    }
}
