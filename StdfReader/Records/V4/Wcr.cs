// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Wcr : StdfRecord {

        Wcr(byte[] data, Endian endian) {
            WaferSize = 0;
            DieHeight = 0;
            DieWidth = 0;
            Units = 0;

            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 4) >= 0) this.WaferSize = rd.ReadSingle();
                if ((i -= 4) >= 0) this.DieHeight = rd.ReadSingle();
                if ((i -= 4) >= 0) this.DieWidth = rd.ReadSingle();
                if ((i -= 1) >= 0) this.Units = rd.ReadByte();
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.Flat = x;
                }
                if ((i -= 1) >= 0) this.Flat = rd.ReadCharacter().ToString();
                if ((i -= 2) >= 0) {
                    var x = rd.ReadInt16();
                    if (x != Int16.MinValue)
                        this.CenterX = x;
                }
                if ((i -= 2) >= 0) {
                    var x = rd.ReadInt16();
                    if (x != Int16.MinValue)
                        this.CenterY = x;
                }
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.PositiveX = x;
                }
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.PositiveY = x;
                }
            }
        }

        public static Wcr Converter(byte[] data, Endian endian) {
            return new Wcr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.WCR; }
        }

        public float? WaferSize { get; set; }
        public float? DieHeight { get; set; }
        public float? DieWidth { get; set; }
        /// <summary>
        /// Known values are: 0 (unknown), 1 (in), 2 (cm), 3 (mm), 4 (mils)
        /// </summary>
        public byte? Units { get; set; }
        /// <summary>
        /// Known values are: U, D, L, R
        /// </summary>
        public string Flat { get; set; }
        public short? CenterX { get; set; }
        public short? CenterY { get; set; }
        /// <summary>
        /// Known values are: L, R
        /// </summary>
        public string PositiveX { get; set; }
        /// <summary>
        /// Known values are: U, D
        /// </summary>
        public string PositiveY { get; set; }
    }
}
