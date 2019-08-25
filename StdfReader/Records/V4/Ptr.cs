// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Ptr : StdfRecord, IHeadSiteIndexable {

        public override RecordType RecordType {
            get { return StdfFile.PTR; }
        }

        Ptr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 4) >= 0) this.TestNumber = rd.ReadUInt32();
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.TestFlags = rd.ReadByte();
                if ((i -= 1) >= 0) this.ParametricFlags = rd.ReadByte();
                if (((TestFlags >> 1) & 0x1) == 0) {
                    if ((i -= 4) >= 0) this.Result = rd.ReadSingle();
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TestText = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.AlarmId = rd.ReadString(length);
                if ((i -= 1) >= 0) this.OptionalFlags = rd.ReadByte();

                if ((i -= 1) >= 0) {
                    if (((OptionalFlags >> 0) & 0x1) == 0) 
                        this.ResultScalingExponent = rd.ReadSByte();
                    else
                        rd.Skip1();
                }
                if ((i -= 1) >= 0) { 
                    if ((OptionalFlags & 0b01010000) == 0) 
                        this.LowLimitScalingExponent = rd.ReadSByte();                    
                    else
                        rd.Skip1();
                }
                if ((i -= 1) >= 0) {
                    if ((OptionalFlags & 0b10100000) == 0) 
                        this.HighLimitScalingExponent = rd.ReadSByte();
                    else
                        rd.Skip1();
                }

                if ((i -= 4) >= 0) {
                    if ((OptionalFlags & 0b01010000) == 0)
                        this.LowLimit = rd.ReadSingle();
                    else
                        rd.Skip4();
                }
                if ((i -= 4) >= 0) {
                    if ((OptionalFlags & 0b10100000) == 0)
                        this.HighLimit = rd.ReadSingle();
                    else
                        rd.Skip4();
                }

                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.Units = rd.ReadString(length);
                //length = 0;
                //if ((i -= 1) >= 0) length = rd.ReadByte();
                //if ((i -= length) >= 0 && length > 0) this.ResultFormatString = rd.ReadString(length);
                //length = 0;
                //if ((i -= 1) >= 0) length = rd.ReadByte();
                //if ((i -= length) >= 0 && length > 0) this.LowLimitFormatString = rd.ReadString(length);
                //length = 0;
                //if ((i -= 1) >= 0) length = rd.ReadByte();
                //if ((i -= length) >= 0 && length > 0) this.HighLimitFormatString = rd.ReadString(length);
                //if (((OptionalFlags >> 2) & 0x1) == 0) {
                //    if ((i -= 4) >= 0) this.LowSpecLimit = rd.ReadSingle();
                //}
                //if (((OptionalFlags >> 3) & 0x1) == 0) {
                //    if ((i -= 4) >= 0) this.HighSpecLimit = rd.ReadSingle();
                //}

            }
        }

        public static Ptr Converter(byte[] data, Endian endian) {
            return new Ptr(data, endian);
        }

        public uint TestNumber { get; set; }
        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
        public byte TestFlags { get; set; }
        public byte ParametricFlags { get; set; }
        public float? Result { get; set; }
        public string TestText { get; set; }
        public string AlarmId { get; set; }
        public byte? OptionalFlags { get; set; }
        /// <summary>
        /// Known values are: 15, 12, 9, 6, 3, 2, 0, -3, -6, -9, -12
        /// </summary>
        public sbyte? ResultScalingExponent { get; set; }
        /// <summary>
        /// Known values are: 15, 12, 9, 6, 3, 2, 0, -3, -6, -9, -12
        /// </summary>
        public sbyte? LowLimitScalingExponent { get; set; }
        /// <summary>
        /// Known values are: 15, 12, 9, 6, 3, 2, 0, -3, -6, -9, -12
        /// </summary>
        public sbyte? HighLimitScalingExponent { get; set; }
        public float? LowLimit { get; set; }
        public float? HighLimit { get; set; }
        public string Units { get; set; }
        public string ResultFormatString { get; set; }
        public string LowLimitFormatString { get; set; }
        public string HighLimitFormatString { get; set; }
        public float? LowSpecLimit { get; set; }
        public float? HighSpecLimit { get; set; }

        public bool ifPassed {
            get {
                return ((ParametricFlags & 0x40) > 0 && (ParametricFlags & 0x80) == 0);
            }
        }
    }
}
