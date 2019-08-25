// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Wir : StdfRecord, IHeadIndexable {

        Wir(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) {
                    var x = rd.ReadByte();
                    if (x != byte.MaxValue)
                        this.SiteGroup = x;
                }
                if ((i -= 4) >= 0) this.StartTime = rd.ReadDateTime();
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.WaferId = rd.ReadString(length);
            }
        }

        public static Wir Converter(byte[] data, Endian endian) {
            return new Wir(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.WIR; }
        }

        public byte HeadNumber { get; set; }
        public byte? SiteGroup { get; set; }
        public DateTime? StartTime { get; set; }
        public string WaferId { get; set; }
    }
}
