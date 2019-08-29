using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    /// <summary>
    /// In a pair of PIR=>PRR, if the TN and TestText are same as the previous one with same site, think it as a sub test, assign a new subNumber
    /// </summary>
    public struct TestID:IEquatable<TestID> {
        public uint MainNumber { get; private set; }
        public string TestText { get; private set; }
        public uint SubNumber { get; private set; }

        public TestID(uint testNumber, string testText) : this(testNumber, testText, 0) { }

        public TestID(uint testNumber, string testText, uint subNumber) {
            MainNumber = testNumber;
            TestText = testText;
            SubNumber = subNumber;
        }
        public static TestID NewSubTestID(TestID testID) {
            return new TestID(testID.MainNumber, testID.TestText, testID.SubNumber++);
        }

        /// <summary>
        /// intended to compare this testID with the coimg new TN(consist of TN and testText)
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="testText"></param>
        /// <returns></returns>
        public bool CompareTestNumber(uint tn, string testText) {
            return MainNumber == tn && TestText == testText;
        }

        public override int GetHashCode() {
            return ((int)MainNumber) ^ TestText.GetHashCode() |((int)SubNumber<<20);
        }

        public bool Equals(TestID id) {
            //this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, id)) return false;

            //如果为同一对象，必然相等
            if (ReferenceEquals(this, id)) return true;

            return id.GetHashCode()==this.GetHashCode();
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
