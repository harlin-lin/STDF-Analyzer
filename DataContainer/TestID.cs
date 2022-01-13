using DataContainer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace DataContainer{
    public struct TempID /*: IComparer */{
        public uint TestNumber { get; }
        public string TestName { get; }
        private int _hashCode;

        public TempID(uint tn, string name) {
            TestNumber = tn;
            TestName = name;
            if (SillyMonkeySetup.IfCmpTextInUid)
                _hashCode = $"{TestNumber}_{TestName}".GetHashCode();
            else
                _hashCode = "TestNumber".GetHashCode();
        }

        public override int GetHashCode() {
            return _hashCode;
        }

        //public int Compare(object x, object y) {
        //    if (!(x is TempID && y is TempID)) throw new Exception("aaa");
        //    return (int)(((TempID)x).TestNumber - ((TempID)y).TestNumber);
        //}
    }

    /// <summary>
    /// In a pair of PIR=>PRR, if the TN and TestText are same as the previous one with same site, think it as a sub test, assign a new subNumber
    /// </summary>
    [Serializable]
    public struct TestID:IEquatable<TestID> {
        public uint TestNumber { get; }
        public int SubID{ get; }
        public string TestName { get; }
        public string UID { get; }
        private int _hashCode;

        public TestID(TestID id) : this(id.TestNumber, id.SubID+1, id.TestName) { }

        public TestID(TempID tempID, int subId) : this(tempID.TestNumber, subId, tempID.TestName) { }

        public TestID(TempID tempID) : this(tempID.TestNumber, 0, tempID.TestName) { }

        public TestID(uint testNumber, string text) : this(testNumber, 0, text) { }

        public TestID(uint testNumber, int subNumber, string text) {
            TestNumber = testNumber;
            SubID = subNumber;
            TestName = text;
            if(SillyMonkeySetup.IfCmpTextInUid)
                UID = $"{testNumber}_{subNumber}_{text}";
            else
                UID = $"{testNumber}_{subNumber}";
            _hashCode = UID.GetHashCode();
        }

        public bool IfSubTest(TempID id) {
            if (SillyMonkeySetup.IfCmpTextInUid) {
                return id.TestNumber == TestNumber && id.TestName == TestName;
            } else {
                return id.TestNumber == TestNumber;
            }
        }
        public bool IfSubTest(uint testNumber, string text) {
            if (TestName is null) return false;
            if (SillyMonkeySetup.IfCmpTextInUid) {
                return testNumber == TestNumber && text == TestName;
            } else {
                return testNumber == TestNumber;
            }
        }
        public string GetUID() {
            return UID;
        }

        public override string ToString() {
            return UID;
        }

        public static TestID NewSubTestID(TestID testID) {
            return new TestID(testID.TestNumber, testID.SubID + 1, testID.TestName);
        }

        /// <summary>
        /// intended to compare this testID with the coimg new TN(consist of TN and testText)
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="testText"></param>
        /// <returns></returns>
        public bool CompareTestNumber(uint tn) {
            return TestNumber == tn;
        }

        public override int GetHashCode() {
            return _hashCode;
        }

        public bool Equals(TestID id) {
            //this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, id)) return false;

            //如果为同一对象，必然相等
            if (ReferenceEquals(this, id)) return true;

            return id._hashCode == this._hashCode;
        }

        public override bool Equals(object obj) {
            //this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, obj)) return false;

            //如果为同一对象，必然相等
            if (ReferenceEquals(this, obj)) return true;

            //如果类型不同，则必然不相等
            if (obj.GetType() != this.GetType()) return false;

            //调用强类型对比
            return Equals((TestID)obj);
        }

        public static bool operator ==(TestID left, TestID right) {
            return left.Equals(right);
        }

        public static bool operator !=(TestID left, TestID right) {
            return !left.Equals(right);
        }

    }
}
