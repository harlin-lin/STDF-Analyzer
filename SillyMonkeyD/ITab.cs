using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SillyMonkeyD {
    interface ITab {
        string TabTitle { get; }
        int FilterId { get; }
        IDataAcquire DataAcquire { get; }
        void UpdateFilter();
    }
}
