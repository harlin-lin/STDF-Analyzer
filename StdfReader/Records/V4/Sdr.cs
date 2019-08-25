// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Sdr : StdfRecord, IHeadIndexable {

        Sdr(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 1) >= 0) this.HeadNumber = rd.ReadByte();
                if ((i -= 1) >= 0) this.SiteGroup = rd.ReadByte();
                byte siteCount = 0;
                if ((i -= 1) >= 0) siteCount = rd.ReadByte();
                if (siteCount > 0) {
                    if ((i -= siteCount) >= 0)
                        this.SiteNumbers = rd.ReadByteArray(siteCount);
                    else
                        throw new Exception("Stdf Data Error!");
                }
                else {
                    throw new Exception("Stdf Data Error!");
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.HandlerType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.HandlerId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.CardType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.CardId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.LoadboardType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.LoadboardId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.DibType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.DibId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.CableType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.CableId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ContactorType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ContactorId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.LaserType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.LaserId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ExtraType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ExtraId = rd.ReadString(length);
            }
        }

        public static Sdr Converter(byte[] data, Endian endian) {
            return new Sdr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.SDR; }
        }

        public byte HeadNumber { get; set; }
        public byte? SiteGroup { get; set; }
        public byte[] SiteNumbers { get; set; }
        public string HandlerType { get; set; }
        public string HandlerId { get; set; }
        public string CardType { get; set; }
        public string CardId { get; set; }
        public string LoadboardType { get; set; }
        public string LoadboardId { get; set; }
        public string DibType { get; set; }
        public string DibId { get; set; }
        public string CableType { get; set; }
        public string CableId { get; set; }
        public string ContactorType { get; set; }
        public string ContactorId { get; set; }
        public string LaserType { get; set; }
        public string LaserId { get; set; }
        public string ExtraType { get; set; }
        public string ExtraId { get; set; }

        [Obsolete("Sdr.Sites has been renamed Sdr.SiteNumbers to be consistent other records")]
        public byte[] Sites {
            get { return SiteNumbers; }
            set { SiteNumbers = value; }
        }
    }
}
