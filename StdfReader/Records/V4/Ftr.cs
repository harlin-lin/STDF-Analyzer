// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace StdfReader.Records.V4 {

    public class Ftr : StdfRecord, IHeadSiteIndexable  {

        Ftr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 4) >= 0) this.TestNumber = rd.ReadUInt32(); 
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.TestFlags = rd.ReadByte();
                if ((i -= 1) >= 0) rd.Skip1();
                if ((i -= 4) >= 0) rd.Skip4();
                if ((i -= 4) >= 0) rd.Skip4();
                if ((i -= 4) >= 0) rd.Skip4();
                if ((i -= 4) >= 0) rd.Skip4();
                if ((i -= 4) >= 0) rd.Skip4();
                if ((i -= 4) >= 0) rd.Skip4();
                if ((i -= 2) >= 0) rd.Skip2();
                ushort rtnCnt = 0;
                if ((i -= 2) >= 0) rtnCnt = rd.ReadUInt16();
                ushort pgmCnt = 0;
                if ((i -= 2) >= 0) pgmCnt = rd.ReadUInt16();
                if (rtnCnt > 0) {
                    if ((i -= 2 * rtnCnt) >= 0)
                        rd.Skip2Array(rtnCnt);
                    else
                        throw new Exception("Stdf Data Error!");
                    if ((i -= (int)Math.Ceiling((double)(rtnCnt / 2))) >= 0)
                        rd.SkipNibbleArray((byte)rtnCnt);
                    else
                        throw new Exception("Stdf Data Error!");
                }
                if (pgmCnt > 0) {
                    if ((i -= 2 * pgmCnt) >= 0)
                        rd.Skip2Array(pgmCnt);
                    else
                        throw new Exception("Stdf Data Error!");
                    if ((i -= (int)Math.Ceiling((double)(pgmCnt / 2))) >= 0)
                        rd.SkipNibbleArray((byte)pgmCnt);
                    else
                        throw new Exception("Stdf Data Error!");
                }
                if ((i -= 2) >= 0) {
                    BitArray FailingPinBitfield = rd.ReadBitArray();
                    if(FailingPinBitfield!=null)
                        i -= ((FailingPinBitfield.Length + 7) / 8);
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) rd.Skip1Array(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) rd.Skip1Array(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) rd.Skip1Array(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TestText = rd.ReadString(length);
                //length = 0;
                //if ((i -= 1) >= 0) length = rd.ReadByte();
                //if ((i -= length) >= 0 && length > 0) this.AlarmId = rd.ReadString(length);
                ////length = 0;
                ////if ((i -= 1) >= 0) length = rd.ReadByte();
                ////if ((i -= length) >= 0 && length > 0) this.ProgrammedText = rd.ReadString(length);
                ////length = 0;
                ////if ((i -= 1) >= 0) length = rd.ReadByte();
                ////if ((i -= length) >= 0 && length > 0) this.ResultText = rd.ReadString(length);
                //if ((i -= 1) >= 0) {
                //    var x = rd.ReadByte();
                //    if (x != byte.MaxValue)
                //        this.PatternGeneratorNumber = x;
                //}
                //if ((i -= 2) >= 0) this.SpinMap = rd.ReadBitArray(); 

            }
        }

        public static Ftr Converter(byte[] data, Endian endian) {
            return new Ftr(data, endian);
        }

        public override RecordType RecordType {
			get { return StdfFile.FTR; }
		}


        public uint TestNumber { get; set; }
        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
        public byte TestFlags { get; set; }
        public int Results { get {
            return ((TestFlags & 0x40) == 0 && (TestFlags & 0x80) == 0) ? 1 : 0;
            } }
        //      public byte OptionalFlags { get; set; }
        //      public uint? CycleCount { get; set; }
        //      public uint? RelativeVectorAddress { get; set; }
        //      public uint? RepeatCount { get; set; }
        //public uint? FailingPinCount { get; set; }
        //public int? XFailureAddress { get; set; }
        //public int? YFailureAddress { get; set; }
        //public short? VectorOffset { get; set; }
        //public ushort[] ReturnIndexes { get; set; }
        //public byte[] ReturnStates { get; set; }
        //public ushort[] ProgrammedIndexes { get; set; }
        //public byte[] ProgrammedStates { get; set; }
        //public BitArray FailingPinBitfield { get; set; }
        //public string VectorName { get; set; }
        //      public string TimeSetName { get; set; }
        //      public string OpCode { get; set; }
        public string TestText { get; set; }
        //public string AlarmId { get; set; }
        //public string ProgrammedText { get; set; }
        //public string ResultText { get; set; }
        //public byte? PatternGeneratorNumber { get; set; }
        //      public BitArray SpinMap { get; set; }
    }
}
