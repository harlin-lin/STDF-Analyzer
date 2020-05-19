using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public enum RstType {
        Pass=0x3,
        Fail_LL=0x1,
        Fail_GH=0x2,
    }
    public struct Rst{
        float _val;

        // 0-null, 1-fail&less than lo-limit, 2-fail&greater that hi-limit, 3-pass
        byte _flag;

        public float Value {
            get {
                if (HasValue)
                    return _val;
                else
                    throw new Exception("Rst is Null");
            }
        }

        public float GetValueOrDefault() {
            return _val;
        }

        public Rst(float? value, RstType rstType) {
            if (value.HasValue) {
                _val = value.Value;
                _flag = (byte)rstType;
            } else {
                _val = 0;
                _flag = 0;
            }
        }

        public Rst(float? value, float? ll, float? hl) {
            if (value.HasValue) {
                _val = value.Value;
                _flag = (byte)GetRstType(value.Value, ll, hl);
            } else {
                _val = 0;
                _flag = 0;
            }
        }



        public bool HasValue {
            get { return _flag>0; }
        }

        public bool IsPass{
            get {
                if (!HasValue) throw new Exception("Rst is Null");
                return _flag == 0x3;
            }
        }

        public bool IsGreaterHL{
            get {
                if (!HasValue) throw new Exception("Rst is Null");
                return _flag == 0x2;
            }
        }

        public bool IsLessLL{
            get {
                if (!HasValue) throw new Exception("Rst is Null");
                return _flag == 0x1;
            }
        }

        public RstType GetRstType() {
            if (!HasValue) throw new Exception("Rst is Null");
            switch (_flag) {
                case 0x3: return RstType.Pass;
                case 0x2: return RstType.Fail_GH;
                case 0x1: return RstType.Fail_LL;
                default: throw new Exception("Wrong Rst Type");
            }
        }

        public override string ToString() {
            if (HasValue) {
                return _val.ToString("F4");
            } else {
                return string.Empty;
            }
        }

        public bool IsInLimit(float? ll, float? hl) {
            if (HasValue) {
                if (ll.HasValue && _val < ll) return false;
                if (hl.HasValue && _val > ll) return false;
                return true;
            } else {
                throw new Exception("Rst is Null");
            }
        }

        public static RstType GetRstType(float rst, float? ll, float? hl) {
            if (ll.HasValue && rst< ll.Value) {
                return RstType.Fail_LL;
            }
            if (hl.HasValue && rst > hl.Value) {
                return RstType.Fail_GH;
            }
            return RstType.Pass;
        }

    }
}
