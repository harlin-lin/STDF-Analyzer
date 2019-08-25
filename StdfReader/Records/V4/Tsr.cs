// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Tsr : StdfRecord, IHeadSiteIndexable {

        Tsr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.TestType = rd.ReadCharacter().ToString();
                if ((i -= 4) >= 0) this.TestNumber = rd.ReadUInt32();
                if ((i -= 4) >= 0) {
                    var x = rd.ReadUInt32();
                    if (x != UInt32.MaxValue)
                        this.ExecutedCount = x;
                }
                if ((i -= 4) >= 0) {
                    var x = rd.ReadUInt32();
                    if (x != UInt32.MaxValue)
                        this.FailedCount = x;
                }
                if ((i -= 4) >= 0) {
                    var x = rd.ReadUInt32();
                    if (x != UInt32.MaxValue)
                        this.AlarmCount = x;
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TestName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.SequencerName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TestLabel = rd.ReadString(length);
                if ((i -= 1) >= 0) {
                    this.OptionalFlags = rd.ReadByte();
                    if (((OptionalFlags >> 2) & 0x1) == 0) {
                        if ((i -= 4) >= 0) this.TestTime = rd.ReadSingle();
                    }
                    if (((OptionalFlags >> 0) & 0x1) == 0) {
                        if ((i -= 4) >= 0) this.TestMin = rd.ReadSingle();
                    }
                    if (((OptionalFlags >> 1) & 0x1) == 0) {
                        if ((i -= 4) >= 0) this.TestMax = rd.ReadSingle();
                    }
                    if (((OptionalFlags >> 4) & 0x1) == 0) {
                        if ((i -= 4) >= 0) this.TestSum = rd.ReadSingle();
                    }
                    if (((OptionalFlags >> 5) & 0x1) == 0) {
                        if ((i -= 2) >= 0) this.TestSumOfSquares = rd.ReadSingle();
                    }
                }

            }
        }

        public static Tsr Converter(byte[] data, Endian endian) {
            return new Tsr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.TSR; }
        }

        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
        /// <summary>
        /// Known values are: P, F, M
        /// </summary>
        public string TestType { get; set; }
        public uint TestNumber { get; set; }
        public uint? ExecutedCount { get; set; }
        public uint? FailedCount { get; set; }
        public uint? AlarmCount { get; set; }
        public string TestName { get; set; }
        public string SequencerName { get; set; }
        public string TestLabel { get; set; }
        public byte OptionalFlags { get; set; }
        public float? TestTime { get; set; }
        public float? TestMin { get; set; }
        public float? TestMax { get; set; }
        public float? TestSum { get; set; }
        public float? TestSumOfSquares { get; set; }
    }
}
