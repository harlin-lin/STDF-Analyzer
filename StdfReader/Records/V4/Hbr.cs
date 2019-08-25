// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Hbr : StdfRecord {

        Hbr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
                if ((i -= 2) >= 0) this.BinNumber = rd.ReadUInt16();
                if ((i -= 4) >= 0) this.BinCount = rd.ReadUInt32();
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.BinPassFail = x;
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.BinName = rd.ReadString(length);
            }
        }

        public static Hbr Converter(byte[] data, Endian endian) {
            return new Hbr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.HBR; }
        }

        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
        /// <summary>
        /// While ushort, valid bins must be 0 - 32,767
        /// </summary>
        public ushort BinNumber { get; set; }
        public uint BinCount { get; set; }
        /// <summary>
        /// Known values are P, F
        /// </summary>
        public string BinPassFail { get; set; }
        public string BinName { get; set; }
    }
}
