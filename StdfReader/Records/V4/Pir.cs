// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Pir : StdfRecord, IHeadSiteIndexable {

        Pir(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
            }
        }

        public static Pir Converter(byte[] data, Endian endian) {
            return new Pir(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.PIR; }
        }

        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
    }
}
