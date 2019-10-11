using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse
{
    public class ItemInfo: IItemInfo {
        public string TestText { get; private set; }
        public float? LoLimit { get; private set; }
        public float? HiLimit { get; private set; }
        public string Unit { get; private set; }
        private float _llScale;
        private float _hlScale;
        private float _rstScale;
        
        public ItemInfo(string testText, float? ll, float? hl, string unit, sbyte? llScale, sbyte? hlScale, sbyte? rstScale) {
            TestText = testText;
            string u = unit;
            _hlScale = (float)Math.Pow(10, (llScale ?? 0));
            _llScale = (float)Math.Pow(10, (llScale ?? 0));
            _rstScale = (float)Math.Pow(10, (rstScale ?? 0));

            LoLimit = _llScale * ll;
            HiLimit = _hlScale * hl;

            switch (rstScale) {
                case 15:
                    u = "f" + u;
                    break;
                case 12:
                    u = "p" + u;
                    break;
                case 9:
                    u = "n" + u;
                    break;
                case 6:
                    u = "u" + u;
                    break;
                case 3:
                    u = "m" + u;
                    break;
                case -3:
                    u = "K" + u;
                    break;
                case -6:
                    u = "M" + u;
                    break;
                case -9:
                    u = "G" + u;
                    break;
                case -12:
                    u = "T" + u;
                    break;
                default:
                    break;
            }

            Unit = u;
        }

        public float? GetScaledRst(float? value) {
            if (!value.HasValue)
                return null;

            return _rstScale * value;
        }

        public void SetTestText(string testText){
            TestText = testText;
        }
    }

}
