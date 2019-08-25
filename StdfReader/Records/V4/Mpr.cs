// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Mpr : StdfRecord, IHeadSiteIndexable {

        Mpr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 4) >= 0) this.TestNumber = rd.ReadUInt32();
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.TestFlags = rd.ReadByte();
                if ((i -= 1) >= 0) this.ParametricFlags = rd.ReadByte();
                ushort rtnCnt = 0;
                if ((i -= 2) >= 0) rtnCnt = rd.ReadUInt16();
                ushort rsltCnt = 0;
                if ((i -= 2) >= 0) rsltCnt = rd.ReadUInt16();
                if (rtnCnt > 0) {
                    if ((i -= (int)Math.Ceiling((double)(rtnCnt / 2))) >= 0)
                        this.PinStates = rd.ReadNibbleArray(rtnCnt);
                    else
                        throw new Exception("Stdf Data Error!");
                }
                if (rsltCnt > 0) {
                    if ((i -= 4 * rsltCnt) >= 0)
                        this.Results = rd.ReadSingleArray(rsltCnt);
                    else
                        throw new Exception("Stdf Data Error!");
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TestText = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.AlarmId = rd.ReadString(length);
                if ((i -= 1) >= 0) this.OptionalFlags = rd.ReadByte();

                if ((i -= 1) >= 0) {
                    if ((OptionalFlags & 0b00000001) == 0)
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

                if ((i -= 4) >= 0) {
                    if ((OptionalFlags & 0b00000010) == 0)
                        this.StartingCondition = rd.ReadSingle();
                    else
                        rd.Skip4();
                }
                if ((i -= 4) >= 0) {
                    if ((OptionalFlags & 0b00000010) == 0)
                        this.ConditionIncrement = rd.ReadSingle();
                    else
                        rd.Skip4();
                }
                
                if ((i -= 2*rtnCnt) >= 0) { 
                    if (rtnCnt > 0) 
                        this.PinIndexes = rd.ReadUInt16Array(rtnCnt);
                    else
                        throw new Exception("Stdf Data Error!");
                }
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.Units = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.IncrementUnits = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ResultFormatString = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.LowLimitFormatString = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.HighLimitFormatString = rd.ReadString(length);
                if ((i -= 4) >= 0) {
                    if (((OptionalFlags >> 2) & 0x1) == 0)
                        this.LowSpecLimit = rd.ReadSingle();
                    else
                        rd.Skip4();
                }
                if ((i -= 4) >= 0) {
                    if (((OptionalFlags >> 3) & 0x1) == 0)
                        this.HighSpecLimit = rd.ReadSingle();
                    else
                        rd.Skip4();
                }

            }
        }

        public static Mpr Converter(byte[] data, Endian endian) {
            return new Mpr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.MPR; }
        }

        public uint TestNumber { get; set; }
        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
        public byte TestFlags { get; set; }
        public byte ParametricFlags { get; set; }
        public byte[] PinStates { get; set; }
        public float[] Results { get; set; }
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
        public float? StartingCondition { get; set; }
        public float? ConditionIncrement { get; set; }
        public ushort[] PinIndexes { get; set; }
        public string Units { get; set; }
        public string IncrementUnits { get; set; }
        public string ResultFormatString { get; set; }
        public string LowLimitFormatString { get; set; }
        public string HighLimitFormatString { get; set; }
        public float? LowSpecLimit { get; set; }
        public float? HighSpecLimit { get; set; }
    }
}
