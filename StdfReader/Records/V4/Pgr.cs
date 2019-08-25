// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Pgr : StdfRecord {

        Pgr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 2) >= 0) this.GroupIndex = rd.ReadUInt16();
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.GroupName = rd.ReadString(length);
                ushort IndexCount = 0;
                if ((i -= 2) >= 0) IndexCount = rd.ReadUInt16();
                if (IndexCount > 0) {
                    if ((i -= 2 * IndexCount) >= 0)
                        this.PinIndexes = rd.ReadUInt16Array(IndexCount);
                    else
                        throw new Exception("Stdf Data Error!");
                }
            }
        }

        public static Pgr Converter(byte[] data, Endian endian) {
            return new Pgr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.PGR; }
        }

        /// <summary>
        /// While ushort, valid PGR Indexes must be 32,768 - 65,535
        /// </summary>
        public ushort GroupIndex { get; set; }
        public string GroupName { get; set; }
        public ushort[] PinIndexes { get; set; }
    }
}
