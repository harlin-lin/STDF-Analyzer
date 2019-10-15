using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    /// <summary>
    /// In a pair of PIR=>PRR, if the TN and TestText are same as the previous one with same site, think it as a sub test, assign a new subNumber
    /// </summary>
    [Serializable]
    public struct TestID:IEquatable<TestID> {
        public uint MainNumber { get; private set; }
        public uint SubNumber { get; private set; }

        public TestID(uint testNumber) : this(testNumber, 0) { }

        public TestID(uint testNumber, uint subNumber) {
            MainNumber = testNumber;
            SubNumber = subNumber;
        }
        public static TestID NewSubTestID(TestID testID) {
            return new TestID(testID.MainNumber, ++testID.SubNumber);
        }

        /// <summary>
        /// intended to compare this testID with the coimg new TN(consist of TN and testText)
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="testText"></param>
        /// <returns></returns>
        public bool CompareTestNumber(uint tn) {
            return MainNumber == tn;
        }

        public override int GetHashCode() {
            return (MainNumber.GetHashCode() ^SubNumber.GetHashCode());
        }

        public bool Equals(TestID id) {
            //this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, id)) return false;

            //如果为同一对象，必然相等
            if (ReferenceEquals(this, id)) return true;

            return id.MainNumber==this.MainNumber && id.SubNumber==this.SubNumber;
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
