using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using DataContainer;

namespace FileReader {
    public partial class StdV4Reader : IDisposable {
        //#region parse Byte
        //private bool Bit(byte flag, byte bit) {
        //    return ((flag >> bit) & 0x1) == 0x1;
        //}
        //private string rdCx(byte[] record, ushort i, ushort len, ushort charCnt) {
        //    if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
        //    //return BitConverter.ToString(record, i, charCnt);
        //    return Encoding.ASCII.GetString(record, i, charCnt);
        //}
        //private string rdCn(byte[] record, ushort i, ushort len) {
        //    if (i > len) throw new Exception("wrong record index");
        //    byte charCnt = record[i];
        //    if (charCnt == 0) return "";
        //    if ((i + charCnt) > len) throw new Exception("wrong record index");
        //    //return BitConverter.ToString(record, i+1, charCnt);
        //    return Encoding.ASCII.GetString(record, i + 1, charCnt);
        //}
        //private string rdCf(byte[] record, ushort i, ushort len, ushort charCnt) {
        //    if ((i + charCnt) > len) throw new Exception("wrong record index");
        //    //return BitConverter.ToString(record, i, charCnt);
        //    return Encoding.ASCII.GetString(record, i, charCnt);
        //}
        //private byte rdU1(byte[] record, ushort i, ushort len) {
        //    if (i > len) throw new Exception("wrong record index");
        //    return record[i];
        //}
        //private ushort rdU2(byte[] record, ushort i, ushort len) {
        //    if ((i + 1) > len) throw new Exception("wrong record index");
        //    return BitConverter.ToUInt16(record, i);
        //}
        //private uint rdU4(byte[] record, ushort i, ushort len) {
        //    if ((i + 3) > len) throw new Exception("wrong record index");
        //    return BitConverter.ToUInt32(record, i);
        //}
        ///// <summary>
        ///// Reads an STDF datetime (4-byte integer seconds since the epoch)
        ///// </summary>
        //private DateTime rdDateTime(byte[] record, ushort i, ushort len) {
        //    var seconds = rdU4(record, i, len);
        //    return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds((double)seconds);
        //}

        //private sbyte rdI1(byte[] record, ushort i, ushort len) {
        //    if (i > len) throw new Exception("wrong record index");
        //    return (sbyte)(record[i]);
        //}
        //private short rdI2(byte[] record, ushort i, ushort len) {
        //    if ((i + 1) > len) throw new Exception("wrong record index");
        //    return BitConverter.ToInt16(record, i);
        //}
        //private int rdI4(byte[] record, ushort i, ushort len) {
        //    if ((i + 3) > len) throw new Exception("wrong record index");
        //    return BitConverter.ToInt32(record, i);
        //}
        //private float rdR4(byte[] record, ushort i, ushort len) {
        //    if ((i + 3) > len) throw new Exception("wrong record index");
        //    return BitConverter.ToSingle(record, i);
        //}
        //private double rdR8(byte[] record, ushort i, ushort len) {
        //    if ((i + 7) > len) throw new Exception("wrong record index");
        //    return BitConverter.ToDouble(record, i);
        //}
        //private byte rdB1(byte[] record, ushort i, ushort len) {
        //    if (i > len) throw new Exception("wrong record index");
        //    return record[i];
        //}
        //private byte[] rdBx(byte[] record, ushort i, ushort len, ushort byteCnt) {
        //    if ((i + byteCnt - 1) > len) throw new Exception("wrong record index");
        //    byte[] rst = new byte[byteCnt];
        //    for (byte j = 0; j < byteCnt; j++) {
        //        rst[j] = record[i + j];
        //    }
        //    return rst;
        //}
        //private byte[] rdBn(byte[] record, ushort i, ushort len) {
        //    if (i > len) throw new Exception("wrong record index");
        //    byte byteCnt = record[i];
        //    if ((i + byteCnt) > len) throw new Exception("wrong record index");
        //    byte[] rst = new byte[byteCnt];
        //    for (byte j = 0; j < byteCnt; j++) {
        //        rst[j] = record[i + j];
        //    }
        //    return rst;
        //}
        //private byte[] rdVn(byte[] record, ushort i, ushort len) {
        //    if (i > len) throw new Exception("wrong record index");
        //    byte byteCnt = record[i];
        //    if ((i + byteCnt) > len) throw new Exception("wrong record index");
        //    byte[] rst = new byte[byteCnt];
        //    for (byte j = 0; j < byteCnt; j++) {
        //        rst[j] = record[i + j];
        //    }
        //    return rst;
        //}
        //private byte[] rdDn(byte[] record, ushort i, ushort len) {
        //    if ((i + 1) > len) throw new Exception("wrong record index");
        //    ushort bitCnt = BitConverter.ToUInt16(record, i);
        //    var byteCnt = bitCnt / 8 + ((bitCnt % 8) > 0 ? 1 : 0);
        //    if ((i + byteCnt) > len) throw new Exception("wrong record index");
        //    byte[] rst = new byte[byteCnt];
        //    for (byte j = 0; j < byteCnt; j++) {
        //        rst[j] = record[i + j];
        //    }
        //    return rst;
        //}

        //private ushort skipDn(byte[] record, ushort i, ushort len) {
        //    if ((i + 1) > len) throw new Exception("wrong record index");
        //    ushort bitCnt = BitConverter.ToUInt16(record, i);
        //    var byteCnt = bitCnt / 8 + ((bitCnt % 8) > 0 ? 1 : 0);
        //    return (ushort)(byteCnt + 2);
        //}

        //private byte[] rdNx(byte[] record, ushort i, ushort len, ushort nibbleCnt) {
        //    if ((i + nibbleCnt - 1) > len) throw new Exception("wrong record index");
        //    byte[] rst = new byte[nibbleCnt];
        //    for (byte j = 0; j < nibbleCnt; j++) {
        //        if (j % 2 == 0) {
        //            rst[j] = (byte)(record[i + j / 2] & 0xf);
        //        } else {
        //            rst[j] = (byte)((record[i + j / 2] & 0xf0) >> 4);
        //        }
        //    }
        //    return rst;
        //}

        //private ushort[] rdKxU2(byte[] record, ushort i, ushort len, ushort cnt) {
        //    if ((i + cnt * 2 - 1) > len) throw new Exception("wrong record index");
        //    ushort[] rst = new ushort[cnt];
        //    for (int j = 0; j < cnt; j++) {
        //        rst[j] = BitConverter.ToUInt16(record, (i + j * 2));
        //    }
        //    return rst;
        //}
        //private byte[] rdKxU1(byte[] record, ushort i, ushort len, ushort cnt) {
        //    if ((i + cnt - 1) > len) throw new Exception("wrong record index");
        //    byte[] rst = new byte[cnt];
        //    for (int j = 0; j < cnt; j++) {
        //        rst[j] = record[i + j];
        //    }
        //    return rst;
        //}
        //private string[] rdKxCn(byte[] record, ushort i, ushort len, ushort cnt) {
        //    string[] rst = new string[cnt];
        //    for (int j = 0; j < cnt; j++) {
        //        if (i > len) throw new Exception("wrong record index");
        //        byte charCnt = record[i];
        //        if (charCnt == 0) {
        //            rst[j] = "";
        //            continue;
        //        }
        //        if ((i + charCnt) > len) throw new Exception("wrong record index");
        //        //rst[j] = BitConverter.ToString(record, i + 1, charCnt);
        //        rst[j] = Encoding.ASCII.GetString(record, i, charCnt);
        //        i += (byte)(charCnt + 1);
        //    }
        //    return rst;
        //}
        //private byte[] rdKxN1(byte[] record, ushort i, ushort len, ushort cnt) {
        //    if ((i + cnt - 1) > len) throw new Exception("wrong record index");
        //    byte[] rst = new byte[cnt];
        //    for (int j = 0; j < cnt; j++) {
        //        rst[j] = (byte)(record[i + j] & 0xf);
        //    }
        //    return rst;
        //}
        //private float[] rdKxR4(byte[] record, ushort i, ushort len, ushort cnt) {
        //    if ((i + cnt * 4 - 1) > len) throw new Exception("wrong record index");
        //    float[] rst = new float[cnt];
        //    for (int j = 0; j < cnt; j++) {
        //        rst[j] = BitConverter.ToSingle(record, (i + j * 4));
        //    }
        //    return rst;
        //}
        //#endregion


        #region parse Byte
        private bool Bit(byte flag, byte bit) {
            return ((flag >> bit) & 0x1) == 0x1;
        }
        private string rdCx(byte[] record, ref ushort i, ushort len, ushort charCnt) {
            if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
            var rst = Encoding.ASCII.GetString(record, i, charCnt);
            i += charCnt;
            return rst;
        }
        private void skipCx(ref ushort i, ushort len, ushort charCnt) {
            if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
            i += charCnt;
        }

        private string rdCn(byte[] record, ref ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            byte charCnt = record[i];
            i += 1;
            if (charCnt == 0) return "";
            if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
            var rst = Encoding.ASCII.GetString(record, i, charCnt);
            i += charCnt;
            return rst;
        }
        private void skipCn(byte[] record, ref ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            byte charCnt = record[i];
            i += 1;
            if (charCnt == 0) return;
            if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
            i += charCnt;
        }

        private string rdCf(byte[] record, ref ushort i, ushort len, ushort charCnt) {
            if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
            var rst = Encoding.ASCII.GetString(record, i, charCnt);
            i += charCnt;
            return rst;
        }
        private byte rdU1(byte[] record, ref ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            var rst = record[i];
            i += 1;
            return rst;
        }
        private ushort rdU2(byte[] record, ref ushort i, ushort len) {
            if ((i + 1) > len) throw new Exception("wrong record index");
            var rst = BitConverter.ToUInt16(record, i);
            i += 2;
            return rst;
        }
        private uint rdU4(byte[] record, ref ushort i, ushort len) {
            if ((i + 3) > len) throw new Exception("wrong record index");
            var rst = BitConverter.ToUInt32(record, i);
            i += 4;
            return rst;
        }
        /// <summary>
        /// Reads an STDF datetime (4-byte integer seconds since the epoch)
        /// </summary>
        private DateTime rdDateTime(byte[] record, ref ushort i, ushort len) {
            var seconds = rdU4(record, ref i, len);
            return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds((double)seconds);
        }

        private sbyte rdI1(byte[] record, ref ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            var rst = (sbyte)(record[i]);
            i += 1;
            return rst;
        }
        private short rdI2(byte[] record, ref ushort i, ushort len) {
            if ((i + 1) > len) throw new Exception("wrong record index");
            var rst = BitConverter.ToInt16(record, i);
            i += 2;
            return rst;
        }
        private int rdI4(byte[] record, ref ushort i, ushort len) {
            if ((i + 3) > len) throw new Exception("wrong record index");
            var rst = BitConverter.ToInt32(record, i);
            i += 4;
            return rst;
        }
        private float rdR4(byte[] record, ref ushort i, ushort len) {
            if ((i + 3) > len) throw new Exception("wrong record index");
            var rst = BitConverter.ToSingle(record, i);
            i += 4;
            return rst;
        }
        private double rdR8(byte[] record, ref ushort i, ushort len) {
            if ((i + 7) > len) throw new Exception("wrong record index");
            var rst = BitConverter.ToDouble(record, i);
            i += 8;
            return rst;
        }
        private byte rdB1(byte[] record, ref ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            var rst = record[i];
            i += 1;
            return rst;
        }
        private byte[] rdBx(byte[] record, ref ushort i, ushort len, ushort byteCnt) {
            if ((i + byteCnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[byteCnt];
            for (byte j = 0; j < byteCnt; j++) {
                rst[j] = record[i + j];
            }
            i += byteCnt;
            return rst;
        }
        private byte[] rdBn(byte[] record, ref ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            byte byteCnt = record[i];
            i += 1;
            if (byteCnt == 0) return new byte[0];
            if ((i + byteCnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[byteCnt];
            for (byte j = 0; j < byteCnt; j++) {
                rst[j] = record[i + j];
            }
            i += byteCnt;
            return rst;
        }
        private byte[] rdDn(byte[] record, ref ushort i, ushort len) {
            if ((i + 1) > len) throw new Exception("wrong record index");
            ushort bitCnt = BitConverter.ToUInt16(record, i);
            i += 2;
            var byteCnt = bitCnt / 8 + ((bitCnt % 8) > 0 ? 1 : 0);
            if ((i + byteCnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[byteCnt];
            for (byte j = 0; j < byteCnt; j++) {
                rst[j] = record[i + j];
            }
            i += (ushort)byteCnt;
            return rst;
        }

        private void skipDn(byte[] record, ref ushort i, ushort len) {
            if ((i + 1) > len) throw new Exception("wrong record index");
            ushort bitCnt = BitConverter.ToUInt16(record, i);
            i += 2;
            var byteCnt = bitCnt / 8 + ((bitCnt % 8) > 0 ? 1 : 0);
            if ((i + byteCnt - 1) > len) throw new Exception("wrong record index");
            i += (ushort)byteCnt;
        }

        private byte[] rdNx(byte[] record, ref ushort i, ushort len, ushort nibbleCnt) {
            if ((i + nibbleCnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[nibbleCnt];
            for (byte j = 0; j < nibbleCnt; j++) {
                if (j % 2 == 0) {
                    rst[j] = (byte)(record[i + j / 2] & 0xf);
                } else {
                    rst[j] = (byte)((record[i + j / 2] & 0xf0) >> 4);
                }
            }
            i += nibbleCnt;
            return rst;
        }

        private ushort[] rdKxU2(byte[] record, ref ushort i, ushort len, ushort cnt) {
            if ((i + cnt * 2 - 1) > len) throw new Exception("wrong record index");
            ushort[] rst = new ushort[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = BitConverter.ToUInt16(record, (i + j * 2));
            }
            i += (ushort)(cnt * 2);
            return rst;
        }
        private byte[] rdKxU1(byte[] record, ref ushort i, ushort len, ushort cnt) {
            if ((i + cnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = record[i + j];
            }
            i += cnt;
            return rst;
        }
        private string[] rdKxCn(byte[] record, ref ushort i, ushort len, ushort cnt) {
            string[] rst = new string[cnt];
            for (int j = 0; j < cnt; j++) {
                if (i > len) throw new Exception("wrong record index");
                byte charCnt = record[i];
                i += 1;
                if (charCnt == 0) {
                    rst[j] = "";
                    continue;
                }
                if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
                rst[j] = Encoding.ASCII.GetString(record, i, charCnt);
                i += charCnt;
            }
            return rst;
        }
        private byte[] rdKxN1(byte[] record, ref ushort i, ushort len, ushort cnt) {
            if ((i + cnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = (byte)(record[i + j] & 0xf);
            }
            i += cnt;
            return rst;
        }
        private float[] rdKxR4(byte[] record, ref ushort i, ushort len, ushort cnt) {
            if ((i + cnt * 4 - 1) > len) throw new Exception("wrong record index");
            float[] rst = new float[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = BitConverter.ToSingle(record, (i + j * 4));
            }
            i += (ushort)(cnt * 4);
            return rst;
        }
        #endregion

    }
}
