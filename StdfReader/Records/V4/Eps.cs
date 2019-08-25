// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Text;

namespace StdfReader.Records.V4 {
	
	public class Eps : StdfRecord {

        Eps(byte[] data, Endian endian) {
            
        }

        public static Eps Converter(byte[] data, Endian endian) {
            return new Eps(data, endian);
        }
        public override RecordType RecordType {
			get { return StdfFile.EPS; }
		}
	}
}
