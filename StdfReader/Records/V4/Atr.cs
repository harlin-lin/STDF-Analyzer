// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace StdfReader.Records.V4 {
	public class Atr : StdfRecord {

        Atr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 4) >= 0) this.ModifiedTime = rd.ReadDateTime();
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length>0) this.CommandLine = rd.ReadString(length);
            }
        }

        public static Atr Converter(byte[] data, Endian endian) {
            return new Atr(data, endian);
        }

        public override RecordType RecordType {
			get { return StdfFile.ATR; }
		}

        public DateTime? ModifiedTime { get; set; }
        public string CommandLine { get; set; }
	}
}
