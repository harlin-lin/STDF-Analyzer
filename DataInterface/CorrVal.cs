using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public enum CorrValType {
        Pass,
        Warn,
        Error,
    }
    public struct CorrVal {
        float _val;

        bool _hasValue;

        CorrValType _valType;

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

        public CorrVal(float? value, CorrValType rstType) {
            if (value.HasValue) {
                _val = value.Value;
                _hasValue = true;
                _valType = rstType;
            } else {
                _val = 0;
                _hasValue = false;
                _valType = CorrValType.Error;
            }
        }


        public bool HasValue {
            get { return _hasValue; }
        }


        public CorrValType GetRstType() {
            if (!HasValue) throw new Exception("Rst is Null");
            return _valType;
        }

        public override string ToString() {
            if (HasValue) {
                return _val.ToString("F4");
            } else {
                return string.Empty;
            }
        }

    }
}
