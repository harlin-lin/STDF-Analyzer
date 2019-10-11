using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public interface IItemInfo {
        string TestText { get; }
        float? LoLimit { get; }
        float? HiLimit { get; }
        string Unit { get; }

        float? GetScaledRst(float? value);

        void SetTestText(string testText); 
    }
}
