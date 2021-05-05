using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public struct SubData {
        public IDataAcquire DataAcquire { get; }
        public int FilterId { get; }

        public SubData(IDataAcquire dataAcquire, int filterId) {
            DataAcquire = dataAcquire;
            FilterId = filterId;
        }
    }
}
