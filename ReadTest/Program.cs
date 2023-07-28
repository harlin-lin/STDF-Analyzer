using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadTest {
    class Program {
        static void Main(string[] args) {
            //for(int i=1; i<=12; i++) {
            //    Console.WriteLine(FindFirstRecordOffset(i, 1, 12).ToString());
            //}

            var std = new StdReader(@"C:\Users\linzhang\Desktop\HolaCon WB01_HolaCon_WB01_TA1_FT.prog_25_JVYA25M003-D001_P23U64.02-JTA111_R0_20230720_151449.stdf", StdFileType.STD);
            //var std = new StdReader(@"C:\Users\Harlin\Documents\SillyMonkey\stdfData\ASR5803F_TFMF80.2-DTA010_2141_FT_datalog_20211022134726.stdf", StdFileType.STD);
            std.ExtractStdf();

            Console.ReadKey();
        }

        static int FindFirstRecordOffset(long blkPos, int aa, int bb) {

            int lBound = aa;
            int ubound = bb;
            int i = (int)Math.Ceiling((lBound + ubound) / 2.0);
            while (true) {

                if (i == blkPos) {
                    return i;
                } else if (i < blkPos) {
                    lBound = i;
                    i = (int)Math.Ceiling((ubound + i) / 2.0);
                } else {
                    ubound = i;
                    i = (int)Math.Ceiling((lBound + i) / 2.0);
                }

                if(lBound + 1 == ubound) {
                    lBound--;
                }
            }

        }
    }
}
