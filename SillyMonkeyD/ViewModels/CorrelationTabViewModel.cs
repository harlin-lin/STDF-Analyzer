using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;

namespace SillyMonkeyD.ViewModels {
    public class CorrelationTabViewModel : ViewModelBase, ITab {

        public CorrelationTabViewModel(List<Tuple<IDataAcquire, int>> dataFilterTuple) {
            DataAcquire = null;
            FilterId = 0;
            WindowFlag = 1;

            TabTitle = $"QTY:{dataFilterTuple.Count}-CORR";
            FilePath = "";
            foreach (var v in dataFilterTuple) {
                FilePath += $"{v.Item1.FileName}:{v.Item2}-";
            }

            _dataFilterTuple = dataFilterTuple;

            if (dataFilterTuple.Count == 2) {
                Data = CorrTwoFiles();
            } else {

            }

            InitUI();
        }

        List<Tuple<IDataAcquire, int>> _dataFilterTuple;

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }
        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }

        public DataTable Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }

        DataTable CorrTwoFiles() {
            DataTable table = new DataTable();

            table.Columns.Add("TestNumber");
            table.Columns.Add("TestText");
            table.Columns.Add("LoLimit");
            table.Columns.Add("HiLimit");
            table.Columns.Add("Unit");
            table.Columns.Add("Min1");
            table.Columns.Add("Max1");
            table.Columns.Add("Mean1");
            table.Columns.Add("Sigma1");
            table.Columns.Add("CPK1");
            table.Columns.Add("Min2");
            table.Columns.Add("Max2");
            table.Columns.Add("Mean2");
            table.Columns.Add("Sigma2");
            table.Columns.Add("CPK2");
            table.Columns.Add("Mean Delta");
            table.Columns.Add("Mean Delta%");

            var id1 = _dataFilterTuple[0].Item1.GetFilteredTestIDs_Info(_dataFilterTuple[0].Item2);
            var id2 = _dataFilterTuple[1].Item1.GetFilteredTestIDs_Info(_dataFilterTuple[1].Item2);
            var data1 = _dataFilterTuple[0].Item1.GetFilteredStatistic(_dataFilterTuple[0].Item2);
            var data2 = _dataFilterTuple[1].Item1.GetFilteredStatistic(_dataFilterTuple[1].Item2);

            foreach (var idInfo in id1) {
                List<object> list = new List<object>(17);

                list.Add(idInfo.Key.GetGeneralTestNumber());
                list.Add(idInfo.Value.TestText);
                list.Add(idInfo.Value.LoLimit);
                list.Add(idInfo.Value.HiLimit);
                list.Add(idInfo.Value.Unit);
                list.Add(data1[idInfo.Key].MinValue);
                list.Add(data1[idInfo.Key].MaxValue);
                list.Add(data1[idInfo.Key].MeanValue);
                list.Add(data1[idInfo.Key].Sigma);
                list.Add(data1[idInfo.Key].Cpk);
                if (id2.ContainsKey(idInfo.Key)) {
                    list.Add(data2[idInfo.Key].MinValue);
                    list.Add(data2[idInfo.Key].MaxValue);
                    list.Add(data2[idInfo.Key].MeanValue);
                    list.Add(data2[idInfo.Key].Sigma);
                    list.Add(data2[idInfo.Key].Cpk);

                    try {
                        var d = Math.Abs(data1[idInfo.Key].MeanValue.Value - data2[idInfo.Key].MeanValue.Value);
                        var bw = Math.Abs(idInfo.Value.HiLimit.Value - idInfo.Value.LoLimit.Value);
                        var dp = (d * 100 / bw);
                        list.Add(d);
                        list.Add(dp);
                    } catch {
                        ;
                    }
                }
                table.Rows.Add(list.ToArray());
            }

            //output the ones not in the first file
            foreach (var idInfo in id2) {
                if (id1.ContainsKey(idInfo.Key)) continue;

                List<object> list = new List<object>(17);

                list.Add(idInfo.Key.GetGeneralTestNumber());
                list.Add(idInfo.Value.TestText);
                list.Add(idInfo.Value.LoLimit);
                list.Add(idInfo.Value.HiLimit);
                list.Add(idInfo.Value.Unit);
                list.Add(null);
                list.Add(null);
                list.Add(null);
                list.Add(null);
                list.Add(null);
                list.Add(data2[idInfo.Key].MinValue);
                list.Add(data2[idInfo.Key].MaxValue);
                list.Add(data2[idInfo.Key].MeanValue);
                list.Add(data2[idInfo.Key].Sigma);
                list.Add(data2[idInfo.Key].Cpk);
                list.Add(null);
                list.Add(null);

                table.Rows.Add(list.ToArray());
            }

            return table;
        }

        public void UpdateFilter() {
            if (_dataFilterTuple.Count == 2) {
                Data = CorrTwoFiles();
            } else {

            }

            RaisePropertyChanged("Data");
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }

        private void InitUI() {
            
        }

        #endregion


    }
}