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
        string LotId { get; }
        string PartType { get; }
        string NodeName { get; }
        string TesterType { get; }
        string JobName { get; }
        string JobRevision { get; }
        string SublotId { get; }
        string ExecType { get; }
        string ExecVersion { get; }
        string TestCode { get; }
        string TestTemperature { get; }
        string UserText { get; }
        string FamilyId { get; }
        string DateCode { get; }
        string FacilityId { get; }

    }
}
