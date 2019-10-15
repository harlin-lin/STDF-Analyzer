using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    [Serializable]
    public class ItemStatistic: IItemStatistic {
        public float? MeanValue { get; private set; }
        public float? MinValue { get; private set; }
        public float? MaxValue { get; private set; }
        public double? Cp { get; private set; }
        public double? Cpk { get; private set; }
        public double? Sigma { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }

        public ItemStatistic(List<float?> data, float? ll, float? hl) {
            List<float> listUnNullItems = (from r in data
                                           where r != null
                                           let v = (float)r
                                           select v).ToList();

            if (listUnNullItems.Count != 0) {

                MeanValue = listUnNullItems.Average();
                MinValue = listUnNullItems.Min();
                MaxValue = listUnNullItems.Max();

                //(sum((Xi-Mean)^2)/count)^0.5
                Sigma = Math.Sqrt((from r in listUnNullItems
                                   let a = Math.Pow((double)(r - MeanValue), 2)
                                   select a).Sum() / listUnNullItems.Count);

                double? T = null;
                double? U = null;
                double? Ca = null;
                if (hl != null && ll != null) {
                    T = ((double)hl - (double)ll);
                    U = ((double)hl + (double)ll) / 2;
                    Ca = (MeanValue - U) / (T / 2);
                    //Cp= (Hlimit-Llimit)/(6*Sigma)
                    Cp = (double)(T / (Sigma * 6));
                    //Cpk = Cp*(1-|Ca|)
                    Cpk = Cp * (1 - Math.Abs((double)Ca));
                }
            }

            PassCount = 0;
            FailCount = 0;

            if(!ll.HasValue && !hl.HasValue) {
                PassCount = listUnNullItems.Count;
                FailCount = data.Count - PassCount;
            } else {
                foreach(var v in listUnNullItems) {
                    if ((ll.HasValue || v >= ll) && (hl.HasValue || v <= hl))
                        PassCount++;
                }
                FailCount = data.Count - PassCount;
            }


        }
    }
}
