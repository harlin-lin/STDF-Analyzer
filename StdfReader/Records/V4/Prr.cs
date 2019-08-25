// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Prr : StdfRecord, IHeadSiteIndexable {

        Prr(byte[] data, Endian endian) {
            TestTime = 0;

            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.PartFlag = rd.ReadByte();
                if ((i -= 2) >= 0) this.TestCount = rd.ReadUInt16();
                if ((i -= 2) >= 0) this.HardBin = rd.ReadUInt16();
                if ((i -= 2) >= 0) {
                    var x = rd.ReadUInt16();
                    if (x != UInt16.MaxValue)
                        this.SoftBin = x;
                }
                if ((i -= 2) >= 0) {
                    var x = rd.ReadInt16();
                    if (x != Int16.MinValue)
                        this.XCoordinate = x;
                }
                if ((i -= 2) >= 0) {
                    var x = rd.ReadInt16();
                    if (x != Int16.MinValue)
                        this.YCoordinate = x;
                }
                if ((i -= 4) >= 0) this.TestTime = rd.ReadUInt32();
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.PartId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.PartText = rd.ReadString(length);
                if ((i -= 2) >= 0) this.PartFix = rd.ReadBitArray(); 
            }
        }

        public static Prr Converter(byte[] data, Endian endian) {
            return new Prr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.PRR; }
        }

        public byte HeadNumber { get; set; }
        public byte SiteNumber { get; set; }
        public byte PartFlag { get; set; }
        public ushort TestCount { get; set; }
        /// <summary>
        /// While ushort, valid bins must be 0 - 32,767
        /// </summary>
        public ushort HardBin { get; set; }
        /// <summary>
        /// While ushort, valid bins must be 0 - 32,767
        /// </summary>
        public ushort? SoftBin { get; set; }
        public short? XCoordinate { get; set; }
        public short? YCoordinate { get; set; }
        public uint? TestTime { get; set; }
        public string PartId { get; set; }
        public string PartText { get; set; }
        public BitArray PartFix { get; set; }
        
        
        
        //dependency properties
        static readonly byte _SupersedesPartIdMask = 0x01;
        static readonly byte _SupersedesCoordsMask = 0x02;
        static readonly byte _AbnormalTestMask = 0x04;
        static readonly byte _FailedMask = 0x08;
        static readonly byte _FailFlagInvalidMask = 0x10;

        public bool SupersedesPartId {
            get { return (PartFlag & _SupersedesPartIdMask) != 0; }
            set {
                if (value) PartFlag |= _SupersedesPartIdMask;
                else PartFlag &= (byte)~_SupersedesPartIdMask;
            }
        }

        public bool SupersedesCoords {
            get { return (PartFlag & _SupersedesCoordsMask) != 0; }
            set {
                if (value) PartFlag |= _SupersedesCoordsMask;
                else PartFlag &= (byte)~_SupersedesCoordsMask;
            }
        }

        public bool AbnormalTest {
            get { return (PartFlag & _AbnormalTestMask) != 0; }
            set {
                if (value) PartFlag |= _AbnormalTestMask;
                else PartFlag &= (byte)~_AbnormalTestMask;
            }
        }

        public bool? Failed {
            get { return (PartFlag & _FailFlagInvalidMask) != 0 ? (bool?)null : (PartFlag & _FailedMask) != 0; }
            set {
                if (value == null) {
                    PartFlag &= (byte)~_FailedMask;
                    PartFlag |= _FailFlagInvalidMask;
                }
                else {
                    PartFlag &= (byte)~_FailFlagInvalidMask;
                    if ((bool)value) PartFlag |= _FailedMask;
                    else PartFlag &= (byte)~_FailedMask;
                }
            }
        }
    }
}
