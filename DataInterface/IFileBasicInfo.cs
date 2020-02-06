using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public interface IFileBasicInfo {
        DateTime? SetupTime { get; }
        DateTime? StartTime { get; }
        DateTime? FinishTime { get; }
        byte? StationNumber { get; }
        string TestModeCode { get; }
        string ReTestCode { get; }
        string ProtectionCode { get; }
        ushort? BurnInTime { get; }
        string CommandModeCode { get; }
        string LotId { get; }
        string PartType { get; }
        string NodeName { get; }
        string TesterType { get; }
        string JobName { get; }
        string JobRevision { get; }
        string SublotId { get; }
        string Operator { get; }
        string ExecType { get; }
        string ExecVersion { get; }
        string TestCode { get; }
        string TestTemperature { get; }
        string UserText { get; }
        string AuxiliaryDataFile { get; }
        string PackageType { get; }
        string FamilyId { get; }
        string DateCode { get; }
        string FacilityId { get; }
        string FloorID { get; }
        string ProcessID { get; }
        string OperationFreq { get; }
        string SpecificationName { get; }
        string SpecificationVersion { get; }
        string FlowID { get; }
        string SetupID { get; }
        string DesignRevision { get; }
        string EngineeringID { get; }
        string RomID { get; }
        string SerialNumber { get; }
        string SupervisorID { get; }
    }
}
