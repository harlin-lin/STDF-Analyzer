using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadTest {
    class Program {
        static void Main(string[] args) {
            var std = new StdReader(@"E:\temp\P23U64.02-JTA111\JVYA25M003-D001\HolaCon WB01_HolaCon_WB01_TA1_FT.prog_25_JVYA25M003-D001_P23U64.02-JTA111_R0_20230720_151449.stdf", StdFileType.STD);
            std.ExtractStdf();

            Console.ReadKey();
        }
    }
}
