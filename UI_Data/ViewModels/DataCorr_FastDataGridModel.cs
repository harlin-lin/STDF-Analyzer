using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using DataContainer;
using FastWpfGrid;
using SillyMonkey.Core;

namespace UI_Data.ViewModels {
    public class DataCorr_FastDataGridModel : FastGridModelBase {
        private List<SubData> _subDataList;

        private Color? _cellColor;

        private HashSet<int> _frozenCols = new HashSet<int>();
        private HashSet<int> _frozenRows = new HashSet<int>();

        private List<Item> _testItems = null;

        List<string> _colNames = new List<string>();
        List<IDataAcquire> _allDa = new List<IDataAcquire>();

        private bool _ifIgnoreOutlier = false;
        private int _outlierSigmaRange = 0;

        private CorrItemType _corrItemType = CorrItemType.All;

        private void UpdateColumnRow() {

            int cnt = _subDataList.Count;

            _allDa.Add(StdDB.GetDataAcquire(_subDataList[0].StdFilePath));
            _testItems = _allDa[0].GetFilteredItemStatistic(_subDataList[0].FilterId).ToList();

            for (int i = 1; i < cnt; i++) {
                _allDa.Add(StdDB.GetDataAcquire(_subDataList[i].StdFilePath));
            }

            _colNames.Clear();

            _colNames.Add("Idx");
            _colNames.Add("TestNO");
            _colNames.Add("TestText          ");
            _colNames.Add("LoLimit");
            _colNames.Add("HiLimit");
            _colNames.Add("Unit");

            if (_corrItemType == CorrItemType.All || _corrItemType == CorrItemType.Mean) {
                for (int i = 0; i < cnt; i++) {
                    _colNames.Add("Mean S:" + i);
                }
            }
            if (_corrItemType == CorrItemType.All || _corrItemType == CorrItemType.Min) {
                for (int i = 0; i < cnt; i++) {
                    _colNames.Add("Min S:" + i);
                }
            }
            if (_corrItemType == CorrItemType.All || _corrItemType == CorrItemType.Max) {
                for (int i = 0; i < cnt; i++) {
                    _colNames.Add("Max S:" + i);
                }
            }
            if (_corrItemType == CorrItemType.All || _corrItemType == CorrItemType.Cp) {
                for (int i = 0; i < cnt; i++) {
                    _colNames.Add("Cp S:" + i);
                }
            }
            if (_corrItemType == CorrItemType.All || _corrItemType == CorrItemType.Cpk) {
                for (int i = 0; i < cnt; i++) {
                    _colNames.Add("Cpk S:" + i);
                }
            }
            if (_corrItemType == CorrItemType.All || _corrItemType == CorrItemType.Sigma) {
                for (int i = 0; i < cnt; i++) {
                    _colNames.Add("Sigma S:" + i);
                }
            }
        }

        public DataCorr_FastDataGridModel(List<SubData> subDataList, CorrItemType corrItemType, bool ifIgnoreOutlier, int outlierSigmaRange) {
            _subDataList = new List<SubData>(subDataList);
            _ifIgnoreOutlier = ifIgnoreOutlier;
            _outlierSigmaRange = outlierSigmaRange;
            _corrItemType = corrItemType;

            UpdateColumnRow();

            _frozenCols.Add(0);


        }

        private HashSet<int> _hiddenRows = new HashSet<int>();

        public void FilterColumn(int column, string filterPat) {
            _hiddenRows.Clear();
            if (!string.IsNullOrWhiteSpace(filterPat)) {
                for (int i = 0; i < RowCount; i++) {
                    if (!Regex.IsMatch(GetCellText(i, column), filterPat, RegexOptions.IgnoreCase)) {
                        _hiddenRows.Add(i);
                    }
                }
            }

            NotifyRefresh();
        }


        public void UpdateView(CorrItemType corrItemType, bool ifIgnoreOutlier, int outlierSigmaRange) {
            _ifIgnoreOutlier = ifIgnoreOutlier;
            _outlierSigmaRange = outlierSigmaRange;
            _corrItemType = corrItemType;

            UpdateColumnRow();

            NotifyRefresh();
        }

        public string GetTestId(int row) {
            if (row >= _testItems.Count) return "";
            return _testItems[row].TestNumber;
        }

        public override HashSet<int> GetFrozenColumns(IFastGridView view) {
            return _frozenCols;
        }

        public override HashSet<int> GetFrozenRows(IFastGridView view) {
            return _frozenRows;
        }

        public override HashSet<int> GetHiddenRows(IFastGridView view) {
            return _hiddenRows;
        }

        public override int ColumnHeaderHeight => 20;
        public override int RowHeaderWidth => 33;

        public override string GetColumnHeaderText(int column) {
            return _colNames[column];
        }

        public override string GetRowHeaderText(int row) {
            return (row + 1).ToString();
        }

        public override int ColumnCount {
            get { return _colNames.Count; }
        }

        public override int RowCount {
            get { return _testItems.Count(); }
        }

        public override Color? FontColor {
            get { return _cellColor; }
        }



        //private void UpdateView() {

        //    dt.Rows.Clear();

        //    foreach (var v in baseItem) {
        //        DataRow r = dt.NewRow();
        //        r[0] = v.TestNumber;
        //        r[1] = v.TestText;
        //        r[2] = v.LoLimit;
        //        r[3] = v.HiLimit;
        //        r[4] = v.Unit;
        //        for (int i = 0; i < cnt; i++) {
        //            if (!allDa[i].IfContainsTestId(v.TestNumber)) continue;
        //            var s = allDa[i].GetFilteredStatistic(_subDataList[i].FilterId, v.TestNumber);
        //            r[5 + i] = s.MeanValue;
        //            r[5 + 1 * cnt + i] = s.MinValue;
        //            r[5 + 2 * cnt + i] = s.MaxValue;
        //            r[5 + 3 * cnt + i] = s.Cp;
        //            r[5 + 4 * cnt + i] = s.Cpk;
        //            r[5 + 5 * cnt + i] = s.Sigma;
        //        }
        //        dt.Rows.Add(r);
        //    }

        //    for (int i = 1; i < cnt; i++) {
        //        var appendId = allDa[i].GetTestIDs().Except(allId);
        //        foreach(var uid in appendId) {
        //            DataRow r = dt.NewRow();
        //            var  s = allDa[i].GetFilteredStatistic(_subDataList[i].FilterId, uid);
        //            var v = allDa[i].GetTestInfo(uid);
        //            r[0] = uid;
        //            r[1] = v.TestText;
        //            r[2] = v.LoLimit;
        //            r[3] = v.HiLimit;
        //            r[4] = v.Unit;

        //            r[5 + i] = s.MeanValue;
        //            r[5 + 1 * cnt + i] = s.MinValue;
        //            r[5 + 2 * cnt + i] = s.MaxValue;
        //            r[5 + 3 * cnt + i] = s.Cp;
        //            r[5 + 4 * cnt + i] = s.Cpk;
        //            r[5 + 5 * cnt + i] = s.Sigma;
        //            dt.Rows.Add(r);
        //        }
        //    }

        //    RaisePropertyChanged("TestItems");
        //}

        public override string GetCellText(int row, int column) {
            int cnt = _subDataList.Count;

            switch (column) {
                case 0:
                    return _testItems[row].Idx.ToString();
                case 1:
                    return _testItems[row].TestNumber.ToString();
                case 2:
                    return _testItems[row].TestText.ToString();
                case 3:
                    return getstr(_testItems[row].LoLimit);
                case 4:
                    return getstr(_testItems[row].HiLimit);
                case 5:
                    return _testItems[row].Unit.ToString();
                default:
                    var si = (column - 6) % cnt;
                    var d = (column - 6) / cnt;
                    if (!_allDa[si].GetTestIDs_Info().ContainsKey(_testItems[row].TestNumber)) return "";

                    ItemStatistic s;
                    if (_ifIgnoreOutlier) {
                        s = _allDa[si].GetFilteredStatisticIgnoreOutlier(_subDataList[si].FilterId, _testItems[row].TestNumber, _outlierSigmaRange);
                    } else {
                        s = _allDa[si].GetFilteredStatistic(_subDataList[si].FilterId, _testItems[row].TestNumber);
                    }
                    if (_corrItemType == CorrItemType.All) {

                        switch (d) {
                            case 0:
                                return getstr(s.MeanValue);
                            case 1:
                                return getstr(s.MinValue);
                            case 2:
                                return getstr(s.MaxValue);
                            case 3:
                                return getstr(s.Cp);
                            case 4:
                                return getstr(s.Cpk);
                            case 5:
                                return getstr(s.Sigma);
                        }
                    } else {
                        switch (_corrItemType) {
                            case CorrItemType.Mean:
                                return getstr(s.MeanValue);
                            case CorrItemType.Min:
                                return getstr(s.MinValue);
                            case CorrItemType.Max:
                                return getstr(s.MaxValue);
                            case CorrItemType.Cp:
                                return getstr(s.Cp);
                            case CorrItemType.Cpk:
                                return getstr(s.Cpk);
                            case CorrItemType.Sigma:
                                return getstr(s.Sigma);
                        }
                    }
                    break;

            }
            return "";
        }

        string getstr(float val) {
            if (float.IsPositiveInfinity(val)) {
                return "Inf+";
            } else if (float.IsNegativeInfinity(val)) {
                return "Inf-";
            } else if (float.IsNaN(val)) {
                return "NaN";
            }
            return val.ToString();
        }

        string getstr(float? val) {
            if (val is null) return "";
            return getstr(val.Value);
        }

        public override IFastGridCellBlock GetBlock(int blockIndex) {
            _cellColor = null;

            return this;
        }

        //public override void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled) {
        //    //base.HandleCommand(view, address, commandParameter, ref handled);
        //    //if (commandParameter is string) MessageBox.Show(commandParameter.ToString());
        //}

        //public override void HandleSelectionCommand(IFastGridView view, string command) {
        //    //MessageBox.Show(command);
        //}


    }
}
