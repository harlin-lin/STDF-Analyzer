using DataInterface;
using StdfReader.Records.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public class FileBasicInfo: IFileBasicInfo {
        public DateTime? SetupTime { get; }
        public DateTime? StartTime { get; }
        public DateTime? FinishTime { get; private set; }
        public byte? StationNumber { get; }
        public string TestModeCode { get; }
        public string ReTestCode { get; }
        public string ProtectionCode { get; }
        public ushort? BurnInTime { get; }
        public string CommandModeCode { get; }
        public string LotId { get; }
        public string PartType { get; }
        public string NodeName { get; }
        public string TesterType { get; }
        public string JobName { get; }
        public string JobRevision { get; }
        public string SublotId { get; }
        public string Operator { get; }
        public string ExecType { get; }
        public string ExecVersion { get; }
        public string TestCode { get; }
        public string TestTemperature { get; }
        public string UserText { get; }
        public string AuxiliaryDataFile { get; }
        public string PackageType { get; }
        public string FamilyId { get; }
        public string DateCode { get; }
        public string FacilityId { get; }
        public string FloorID { get; }
        public string ProcessID { get; }
        public string OperationFreq { get; }
        public string SpecificationName { get; }
        public string SpecificationVersion { get; }
        public string FlowID { get; }
        public string SetupID { get; }
        public string DesignRevision { get; }
        public string EngineeringID { get; }
        public string RomID { get; }
        public string SerialNumber { get; }
        public string SupervisorID { get; }


        public FileBasicInfo(Mir mir) {

            SetupTime = mir.SetupTime;
            StartTime = mir.StartTime;
            StationNumber = mir.StationNumber;
            TestModeCode = mir.ModeCode;
            ReTestCode = mir.RetestCode;
            ProtectionCode = mir.ProtectionCode;
            BurnInTime = mir.BurnInTime;
            CommandModeCode = mir.CommandModeCode;
            LotId = mir.LotId;
            PartType = mir.PartType;
            NodeName = mir.NodeName;
            TesterType = mir.TesterType;
            JobName = mir.JobName;
            JobRevision = mir.JobRevision;
            SublotId = mir.SublotId;
            Operator = mir.OperatorName;
            ExecType = mir.ExecType;
            ExecVersion = mir.ExecVersion;
            TestCode = mir.TestCode;
            TestTemperature = mir.TestTemperature;
            UserText = mir.UserText;
            AuxiliaryDataFile = mir.AuxiliaryFile;
            PackageType = mir.PackageType;
            FamilyId = mir.FamilyId;
            DateCode = mir.DateCode;
            FacilityId = mir.FacilityId;
            FloorID = mir.FloorId;
            ProcessID = mir.ProcessId;
            OperationFreq = mir.OperationFrequency;
            SpecificationName = mir.SpecificationName;
            SpecificationVersion = mir.SpecificationVersion;
            FlowID = mir.FlowId;
            SetupID = mir.SetupId;
            DesignRevision = mir.DesignRevision;
            EngineeringID = mir.EngineeringId;
            RomID = mir.RomCode;
            SerialNumber = mir.SerialNumber;
            SupervisorID = mir.SupervisorName;
        }
        public void AddMrr(Mrr mrr) {
            FinishTime = mrr.FinishTime;
        }
    }
}
