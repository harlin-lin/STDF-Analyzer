using StdfReader.Records.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    class FileBasicInfo {
        public DateTime? SetupTime { get; }
        public DateTime? StartTime { get; }
        public DateTime? FinishTime { get; private set; }
        public string LotId { get; }
        public string PartType { get; }
        public string NodeName { get; }
        public string TesterType { get; }
        public string JobName { get; }
        public string JobRevision { get; }
        public string SublotId { get; }
        public string ExecType { get; }
        public string ExecVersion { get; }
        public string TestCode { get; }
        public string TestTemperature { get; }
        public string UserText { get; }
        public string FamilyId { get; }
        public string DateCode { get; }
        public string FacilityId { get; }


        public FileBasicInfo(Mir mir) {

            SetupTime = mir.SetupTime;
            StartTime = mir.StartTime;
            LotId = mir.LotId;
            PartType = mir.PartType;
            NodeName = mir.NodeName;
            TesterType = mir.TesterType;
            JobName = mir.JobName;
            JobRevision = mir.JobRevision;
            SublotId = mir.SublotId;
            ExecType = mir.ExecType;
            ExecVersion = mir.ExecVersion;
            TestCode = mir.TestCode;
            TestTemperature = mir.TestTemperature;
            UserText = mir.UserText;
            FamilyId = mir.FamilyId;
            DateCode = mir.DateCode;
            FacilityId = mir.FacilityId;

        }
        public void AddMrr(Mrr mrr) {
            FinishTime = mrr.FinishTime;
        }
    }
}
