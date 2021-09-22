using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    [Serializable]
    public class ItemStatistic {
        public float? MeanValue { get; private set; }
        public float? MinValue { get; private set; }
        public float? MaxValue { get; private set; }
        public float? Cp { get; private set; }
        public float? Cpk { get; private set; }
        public float? Sigma { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }

        public ItemStatistic(List<float> data, float? ll, float? hl) {
            List<float> listUnNullItems = (from r in data
                                           where r!=float.NaN && r!=float.NegativeInfinity && r != float.PositiveInfinity
                                           select r).ToList();

            if (listUnNullItems.Count != 0) {

                MeanValue = listUnNullItems.Average();
                MinValue = listUnNullItems.Min();
                MaxValue = listUnNullItems.Max();

                //(sum((Xi-Mean)^2)/count)^0.5
                Sigma = (float)Math.Sqrt((from r in listUnNullItems
                                   let a = Math.Pow((double)(r - MeanValue), 2)
                                   select a).Sum() / listUnNullItems.Count);

                float? T = null;
                float? U = null;
                float? Ca = null;
                if (hl != null && ll != null) {
                    T = ((float)hl - (float)ll);
                    U = ((float)hl + (float)ll) / 2;
                    Ca = (MeanValue - U) / (T / 2);
                    //Cp= (Hlimit-Llimit)/(6*Sigma)
                    Cp = (float)(T / (Sigma * 6));
                    //Cpk = Cp*(1-|Ca|)
                    Cpk = Cp * (1 - Math.Abs((float)Ca));
                }
            }

            PassCount = 0;
            FailCount = 0;

            if(!ll.HasValue && !hl.HasValue) {
                PassCount = listUnNullItems.Count;
                FailCount = data.Count - PassCount;
            } else {
                foreach(var v in listUnNullItems) {
                    if (ll.HasValue && !hl.HasValue){
                        if(v>=ll)
                            PassCount++;
                    }else if(!ll.HasValue && hl.HasValue) {
                        if (v <= hl)
                            PassCount++;
                    } else {
                        if (v >= ll && v<=hl)
                            PassCount++;
                    }
                }
                FailCount = data.Count - PassCount;
            }


        }
    }
}
