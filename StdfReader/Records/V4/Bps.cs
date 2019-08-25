// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
	
	public class Bps : StdfRecord {
        Bps(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.Name = rd.ReadString(length);
            }
        }

        public static Bps Converter(byte[] data, Endian endian) {
            return new Bps(data, endian);
        }

        public override RecordType RecordType {
			get { return StdfFile.BPS; }
		}
        public string Name { get; set; }
	}
}
