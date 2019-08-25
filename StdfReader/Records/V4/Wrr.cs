// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Wrr : StdfRecord, IHeadIndexable {

        Wrr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) {
                    var x = rd.ReadByte();
                    if (x != byte.MaxValue)
                        this.SiteGroup = x;
                }
                if ((i -= 4) >= 0) this.FinishTime = rd.ReadDateTime();
                if ((i -= 4) >= 0) this.PartCount = rd.ReadUInt32();
                if ((i -= 4) >= 0) {
                    var x = rd.ReadUInt32();
                    if (x != UInt32.MaxValue)
                        this.RetestCount = x;
                }
                if ((i -= 4) >= 0) {
                    var x = rd.ReadUInt32();
                    if (x != UInt32.MaxValue)
                        this.AbortCount = x;
                }
                if ((i -= 4) >= 0) {
                    var x = rd.ReadUInt32();
                    if (x != UInt32.MaxValue)
                        this.GoodCount = x;
                }
                if ((i -= 4) >= 0) {
                    var x = rd.ReadUInt32();
                    if (x != UInt32.MaxValue)
                        this.FunctionalCount = x;
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.WaferId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.FabWaferId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.FrameId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.MaskId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.UserDescription = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ExecDescription = rd.ReadString(length);
            }
        }

        public static Wrr Converter(byte[] data, Endian endian) {
            return new Wrr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.WRR; }
        }

        public byte HeadNumber { get; set; }
        public byte? SiteGroup { get; set; }
        public DateTime? FinishTime { get; set; }
        public uint PartCount { get; set; }
        public uint? RetestCount { get; set; }
        public uint? AbortCount { get; set; }
        public uint? GoodCount { get; set; }
        public uint? FunctionalCount { get; set; }
        public string WaferId { get; set; }
        public string FabWaferId { get; set; }
        public string FrameId { get; set; }
        public string MaskId { get; set; }
        public string UserDescription { get; set; }
        public string ExecDescription { get; set; }
    }
}
