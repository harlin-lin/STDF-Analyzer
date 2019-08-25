// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StdfReader.Records.V4 {
    public class Mir : StdfRecord {

        Mir(byte[] data, Endian endian) {
            using (BinaryReader rd = new BinaryReader(new MemoryStream(data), endian, true)) {
                int i = data.Length;
                if ((i -= 4) >= 0) this.SetupTime = rd.ReadDateTime();
                if ((i -= 4) >= 0) this.StartTime = rd.ReadDateTime();
                if ((i -= 1) >= 0) this.StationNumber = rd.ReadByte();
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.ModeCode = x;
                }
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.RetestCode = x;
                }
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.ProtectionCode = x;
                }
                if ((i -= 2) >= 0) {
                    var x = rd.ReadUInt16();
                    if (x != UInt16.MaxValue)
                        this.BurnInTime = x;
                }
                if ((i -= 1) >= 0) {
                    var x = rd.ReadCharacter().ToString();
                    if (x != " ")
                        this.CommandModeCode = x;
                }
                int length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.LotId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.PartType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.NodeName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TesterType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.JobName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.JobRevision = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.SublotId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.OperatorName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ExecType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ExecVersion = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TestCode = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.TestTemperature = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.UserText = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.AuxiliaryFile = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.PackageType = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.FamilyId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.DateCode = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.FacilityId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.FloorId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.ProcessId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.OperationFrequency = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.SpecificationName = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.SpecificationVersion = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.FlowId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.SetupId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.DesignRevision = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.EngineeringId = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.RomCode = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.SerialNumber = rd.ReadString(length);
                length = 0;
                if ((i -= 1) >= 0) length = rd.ReadByte();
                if ((i -= length) >= 0 && length > 0) this.SupervisorName = rd.ReadString(length);
            }
        }

        public static Mir Converter(byte[] data, Endian endian) {
            return new Mir(data, endian);
        }

        public override RecordType RecordType {
            get { return StdfFile.MIR; }
        }

        public DateTime? SetupTime { get; set; }
        public DateTime? StartTime { get; set; }
        public byte StationNumber { get; set; }
        /// <summary>
        /// Known values are: A, C, D, E, M, P, Q, 0-9
        /// </summary>
        public string ModeCode { get; set; }
        /// <summary>
        /// Known values are: Y, N, 0-9
        /// </summary>
        public string RetestCode { get; set; }
        /// <summary>
        /// Known values are A-Z, 0-9
        /// </summary>
        public string ProtectionCode { get; set; }
        public ushort? BurnInTime { get; set; }
        /// <summary>
        /// Known values are A-Z, 0-9
        /// </summary>
        public string CommandModeCode { get; set; }
        public string LotId { get; set; }
        public string PartType { get; set; }
        public string NodeName { get; set; }
        public string TesterType { get; set; }
        public string JobName { get; set; }
        public string JobRevision { get; set; }
        public string SublotId { get; set; }
        public string OperatorName { get; set; }
        public string ExecType { get; set; }
        public string ExecVersion { get; set; }
        public string TestCode { get; set; }
        public string TestTemperature { get; set; }
        public string UserText { get; set; }
        public string AuxiliaryFile { get; set; }
        public string PackageType { get; set; }
        public string FamilyId { get; set; }
        public string DateCode { get; set; }
        public string FacilityId { get; set; }
        public string FloorId { get; set; }
        public string ProcessId { get; set; }
        public string OperationFrequency { get; set; }
        public string SpecificationName { get; set; }
        public string SpecificationVersion { get; set; }
        public string FlowId { get; set; }
        public string SetupId { get; set; }
        public string DesignRevision { get; set; }
        public string EngineeringId { get; set; }
        public string RomCode { get; set; }
        public string SerialNumber { get; set; }
        public string SupervisorName { get; set; }
    }
}
