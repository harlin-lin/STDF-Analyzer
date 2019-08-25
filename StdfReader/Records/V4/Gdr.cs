// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;

namespace StdfReader.Records.V4 {

	public class Gdr : StdfRecord {

        Gdr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                ushort fieldCount = rd.ReadUInt16();
                object[] vData = new object[fieldCount];
                for (int i = 0; i < fieldCount; i++) {
                    byte dataTypeCode = rd.ReadByte();
                    switch (dataTypeCode) {
                        case 0:
                            break;
                        case 1:
                            vData[i] = rd.ReadByte();
                            break;
                        case 2:
                            vData[i] = rd.ReadUInt16();
                            break;
                        case 3:
                            vData[i] = rd.ReadUInt32();
                            break;
                        case 4:
                            vData[i] = rd.ReadSByte();
                            break;
                        case 5:
                            vData[i] = rd.ReadInt16();
                            break;
                        case 6:
                            vData[i] = rd.ReadInt32();
                            break;
                        case 7:
                            vData[i] = rd.ReadSingle();
                            break;
                        case 8:
                            vData[i] = rd.ReadDouble();
                            break;
                        case 10:
                            vData[i] = rd.ReadString();
                            break;
                        case 11: {
                                byte length = rd.ReadByte();
                                byte[] bytes = new byte[length];
                                for (int byteIndex = 0; byteIndex < length; byteIndex++) {
                                    bytes[byteIndex] = rd.ReadByte();
                                }
                                vData[i] = bytes;
                                break;
                            }
                        case 12: {
                                ushort length = rd.ReadUInt16();
                                length = (ushort)((length / 8) + (((length % 8) > 0) ? 1 : 0));
                                byte[] bytes = new byte[length];
                                for (int byteIndex = 0; byteIndex < length; byteIndex++) {
                                    bytes[byteIndex] = rd.ReadByte();
                                }
                                vData[i] = bytes;
                                break;
                            }
                        case 13: {
                                byte nibble = rd.ReadByte();
                                nibble = (byte)(nibble & 0x0F);
                                break;
                            }
                        default:
                            throw new InvalidOperationException(string.Format(Resources.InvalidGdrDataTypeCode, dataTypeCode));
                    }
                }
                this.GenericData = vData;
            }
        }

        public static Gdr Converter(byte[] data, Endian endian) {
            return new Gdr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.GDR; }
        }

		private object[] _GenericData;
		public object[] GenericData {
			get { return _GenericData; }
			set { _GenericData = value; }
		}

    }
}
