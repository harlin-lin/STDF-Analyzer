// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Mrr : StdfRecord {

        Mrr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 4) >= 0) this.FinishTime = rd.ReadDateTime();
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.DispositionCode = x;
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.UserDescription = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ExecDescription = rd.ReadString(length);
            }
        }

        public static Mrr Converter(byte[] data, Endian endian) {
            return new Mrr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.MRR; }
        }

        public DateTime? FinishTime { get; set; }
        public string DispositionCode { get; set; }
        public string UserDescription { get; set; }
        public string ExecDescription { get; set; }
    }
}
