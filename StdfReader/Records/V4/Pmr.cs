// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Pmr : StdfRecord {

        Pmr(byte[] data, Endian endian) {
            ChannelType = 0;
            HeadNumber = 1;
            SiteNumber = 1;
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 2) >= 0) this.PinIndex = rd.ReadUInt16();
                if ((i -= 2) >= 0) this.ChannelType = rd.ReadUInt16();
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ChannelName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.PhysicalName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.LogicalName = rd.ReadString(length);
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
            }
        }

        public static Pmr Converter(byte[] data, Endian endian) {
            return new Pmr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.PMR; }
        }

        /// <summary>
        /// While ushort, valid PMR PinIndexes must be 1 - 32,767
        /// </summary>
        public ushort PinIndex { get; set; }
        public ushort? ChannelType { get; set; }
        public string ChannelName { get; set; }
        public string PhysicalName { get; set; }
        public string LogicalName { get; set; }
        public byte? HeadNumber { get; set; }
        public byte? SiteNumber { get; set; }

    }
}
