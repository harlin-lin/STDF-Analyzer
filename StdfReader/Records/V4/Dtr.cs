// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {

    public class Dtr : StdfRecord {
        Dtr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.Text = rd.ReadString(length);
            }
        }

        public static Dtr Converter(byte[] data, Endian endian) {
            return new Dtr(data, endian);
        }

        public override RecordType RecordType {
			get { return StdfFile.DTR; }
		}

        public string Text { get; set; }
	}
}
