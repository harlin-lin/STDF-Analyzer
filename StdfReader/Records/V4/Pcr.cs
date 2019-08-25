// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace StdfReader.Records.V4 {
    public class Pcr : StdfRecord {

        Pcr(byte[] data, Endian endian) {

            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((--i) >= 0)
                    this.HeadNumber = rd.ReadByte();
                if ((--i) >= 0)
                    this.SiteNumber = rd.ReadByte();
                if ((i -= 4) >= 0)
                    this.PartCount = rd.ReadUInt32();
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
            }
        }

        public static Pcr Converter(byte[] data, Endian endian) {
            return new Pcr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.PCR; }
        }

        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
        public uint PartCount { get; set; }
        public uint? RetestCount { get; set; }
        public uint? AbortCount { get; set; }
        public uint? GoodCount { get; set; }
        public uint? FunctionalCount { get; set; }

        //public Pcr() {
        //    RetestCount = UInt32.MaxValue;
        //    AbortCount = UInt32.MaxValue;
        //    GoodCount = UInt32.MaxValue;
        //    FunctionalCount = UInt32.MaxValue;
        //}

        //internal static Pcr ConvertToPcr(UnknownRecord unknownRecord) {
        //    Pcr pcr = new Pcr();
        //    using (BinaryReader rd = new BinaryReader(new MemoryStream(unknownRecord.Content), unknownRecord.Endian, true)) {
        //        int i = unknownRecord.Content.Length;
        //        if ((--i)>=0)
        //            pcr.HeadNumber = rd.ReadByte();
        //        if ((--i) >= 0)
        //            pcr.SiteNumber = rd.ReadByte();
        //        if ((i-=4) >= 0)
        //            pcr.PartCount = rd.ReadUInt32();
        //        if ((i -= 4) >= 0)
        //            pcr.RetestCount = rd.ReadUInt32();
        //        if ((i -= 4) >= 0)
        //            pcr.AbortCount = rd.ReadUInt32();
        //        if ((i -= 4) >= 0)
        //            pcr.GoodCount = rd.ReadUInt32();
        //        if ((i -= 4) >= 0)
        //            pcr.FunctionalCount = rd.ReadUInt32();
        //    }
        //    return pcr;
        //}

    }
}
