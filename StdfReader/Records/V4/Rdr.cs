// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Rdr : StdfRecord {

        Rdr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                ushort RetestBinsCount=0;
                if ((i -= 2) >= 0) RetestBinsCount = rd.ReadUInt16();
                if(RetestBinsCount > 0) {
                    if ((i -= RetestBinsCount*2) >= 0)
                        this.RetestBins = rd.ReadUInt16Array(RetestBinsCount);
                    else
                        throw new Exception("Stdf Data Error!");
                }
            }
        }

        public static Rdr Converter(byte[] data, Endian endian) {
            return new Rdr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.RDR; }
        }

        public ushort[] RetestBins { get; set; }
    }
}
