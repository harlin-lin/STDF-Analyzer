// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Text;

namespace StdfReader.Records.V4 {

	public class Far : StdfRecord {
        public override RecordType RecordType {
			get { return StdfFile.FAR; }
		}

        public byte CpuType { get; set; }
        public byte StdfVersion { get; set; }
	}
}
