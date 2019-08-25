// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Plr : StdfRecord {

        Plr(byte[] data, Endian endian) {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data), endian, true)) {
                // Group count and list of group indexes are required
                ushort groupCount = reader.ReadUInt16();
                this.GroupIndexes = reader.ReadUInt16Array(groupCount, true);

                // Latter arrays are optional, and may be truncated
                if (!reader.AtEndOfStream) {
                    ushort[] groupModes = reader.ReadUInt16Array(groupCount, false);
                    // Expand a truncated array, filling with missing value
                    if ((groupModes != null) && (groupModes.Length < groupCount)) {
                        int i = groupModes.Length;
                        Array.Resize<ushort>(ref groupModes, groupCount);
                        for (; i < groupModes.Length; i++)
                            groupModes[i] = ushort.MinValue;
                    }
                    this.GroupModes = groupModes;
                }
                if (!reader.AtEndOfStream) {
                    byte[] groupRadixes = reader.ReadByteArray(groupCount, false);
                    // Expand a truncated array, filling with missing value
                    if ((groupRadixes != null) && (groupRadixes.Length < groupCount)) {
                        int i = groupRadixes.Length;
                        Array.Resize<byte>(ref groupRadixes, groupCount);
                        for (; i < groupRadixes.Length; i++)
                            groupRadixes[i] = byte.MinValue;
                    }
                    this.GroupRadixes = groupRadixes;
                }
                if (!reader.AtEndOfStream) {
                    string[] programStatesRight = reader.ReadStringArray(groupCount, false);
                    // Expand a truncated array, filling with missing value
                    if ((programStatesRight != null) && (programStatesRight.Length < groupCount)) {
                        int i = programStatesRight.Length;
                        Array.Resize<string>(ref programStatesRight, groupCount);
                        for (; i < programStatesRight.Length; i++)
                            programStatesRight[i] = "";
                    }
                    this.ProgramStatesRight = programStatesRight;
                }
                if (!reader.AtEndOfStream) {
                    string[] returnStatesRight = reader.ReadStringArray(groupCount, false);
                    // Expand a truncated array, filling with missing value
                    if ((returnStatesRight != null) && (returnStatesRight.Length < groupCount)) {
                        int i = returnStatesRight.Length;
                        Array.Resize<string>(ref returnStatesRight, groupCount);
                        for (; i < returnStatesRight.Length; i++)
                            returnStatesRight[i] = "";
                    }
                    this.ReturnStatesRight = returnStatesRight;
                }
                if (!reader.AtEndOfStream) {
                    string[] programStatesLeft = reader.ReadStringArray(groupCount, false);
                    // Expand a truncated array, filling with missing value
                    if ((programStatesLeft != null) && (programStatesLeft.Length < groupCount)) {
                        int i = programStatesLeft.Length;
                        Array.Resize<string>(ref programStatesLeft, groupCount);
                        for (; i < programStatesLeft.Length; i++)
                            programStatesLeft[i] = "";
                    }
                    this.ProgramStatesLeft = programStatesLeft;
                }
                if (!reader.AtEndOfStream) {
                    string[] returnStatesLeft = reader.ReadStringArray(groupCount, false);
                    // Expand a truncated array, filling with missing value
                    if ((returnStatesLeft != null) && (returnStatesLeft.Length < groupCount)) {
                        int i = returnStatesLeft.Length;
                        Array.Resize<string>(ref returnStatesLeft, groupCount);
                        for (; i < returnStatesLeft.Length; i++)
                            returnStatesLeft[i] = "";
                    }
                    this.ReturnStatesLeft = returnStatesLeft;
                }
            }
        }

        public static Plr Converter(byte[] data, Endian endian) {
            return new Plr(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.PLR; }
        }

        public ushort[] GroupIndexes { get; set; }
        /// <summary>
        /// Known values are: 0, 10, 20, 21, 22, 23, 30, 31, 32, 33
        /// </summary>
        public ushort[] GroupModes { get; set; }
        /// <summary>
        /// Known values are: 0, 2, 8, 10, 16, 20
        /// </summary>
        public byte[] GroupRadixes { get; set; }
        public string[] ProgramStatesRight { get; set; }
        public string[] ReturnStatesRight { get; set; }
        public string[] ProgramStatesLeft { get; set; }
        public string[] ReturnStatesLeft { get; set; }

        //internal static Plr ConvertToPlr(UnknownRecord unknownRecord) {
        //    Plr this = new Plr();
        //    using (BinaryReader reader = new BinaryReader(new MemoryStream(data), unknownRecord.Endian, true)) {
        //        // Group count and list of group indexes are required
        //        ushort groupCount = reader.ReadUInt16();
        //        this.GroupIndexes = reader.ReadUInt16Array(groupCount, true);

        //        // Latter arrays are optional, and may be truncated
        //        if (!reader.AtEndOfStream) {
        //            ushort[] groupModes = reader.ReadUInt16Array(groupCount, false);
        //            // Expand a truncated array, filling with missing value
        //            if ((groupModes != null) && (groupModes.Length < groupCount)) {
        //                int i = groupModes.Length;
        //                Array.Resize<ushort>(ref groupModes, groupCount);
        //                for (; i < groupModes.Length; i++)
        //                    groupModes[i] = ushort.MinValue;
        //            }
        //            this.GroupModes = groupModes;
        //        }
        //        if (!reader.AtEndOfStream) {
        //            byte[] groupRadixes = reader.ReadByteArray(groupCount, false);
        //            // Expand a truncated array, filling with missing value
        //            if ((groupRadixes != null) && (groupRadixes.Length < groupCount)) {
        //                int i = groupRadixes.Length;
        //                Array.Resize<byte>(ref groupRadixes, groupCount);
        //                for (; i < groupRadixes.Length; i++)
        //                    groupRadixes[i] = byte.MinValue;
        //            }
        //            this.GroupRadixes = groupRadixes;
        //        }
        //        if (!reader.AtEndOfStream) {
        //            string[] programStatesRight = reader.ReadStringArray(groupCount, false);
        //            // Expand a truncated array, filling with missing value
        //            if ((programStatesRight != null) && (programStatesRight.Length < groupCount)) {
        //                int i = programStatesRight.Length;
        //                Array.Resize<string>(ref programStatesRight, groupCount);
        //                for (; i < programStatesRight.Length; i++)
        //                    programStatesRight[i] = "";
        //            }
        //            this.ProgramStatesRight = programStatesRight;
        //        }
        //        if (!reader.AtEndOfStream) {
        //            string[] returnStatesRight = reader.ReadStringArray(groupCount, false);
        //            // Expand a truncated array, filling with missing value
        //            if ((returnStatesRight != null) && (returnStatesRight.Length < groupCount)) {
        //                int i = returnStatesRight.Length;
        //                Array.Resize<string>(ref returnStatesRight, groupCount);
        //                for (; i < returnStatesRight.Length; i++)
        //                    returnStatesRight[i] = "";
        //            }
        //            this.ReturnStatesRight = returnStatesRight;
        //        }
        //        if (!reader.AtEndOfStream) {
        //            string[] programStatesLeft = reader.ReadStringArray(groupCount, false);
        //            // Expand a truncated array, filling with missing value
        //            if ((programStatesLeft != null) && (programStatesLeft.Length < groupCount)) {
        //                int i = programStatesLeft.Length;
        //                Array.Resize<string>(ref programStatesLeft, groupCount);
        //                for (; i < programStatesLeft.Length; i++)
        //                    programStatesLeft[i] = "";
        //            }
        //            this.ProgramStatesLeft = programStatesLeft;
        //        }
        //        if (!reader.AtEndOfStream) {
        //            string[] returnStatesLeft = reader.ReadStringArray(groupCount, false);
        //            // Expand a truncated array, filling with missing value
        //            if ((returnStatesLeft != null) && (returnStatesLeft.Length < groupCount)) {
        //                int i = returnStatesLeft.Length;
        //                Array.Resize<string>(ref returnStatesLeft, groupCount);
        //                for (; i < returnStatesLeft.Length; i++)
        //                    returnStatesLeft[i] = "";
        //            }
        //            this.ReturnStatesLeft = returnStatesLeft;
        //        }
        //    }
        //    return this;
        //}

    }
}
