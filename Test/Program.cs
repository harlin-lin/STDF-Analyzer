using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParse;
using System.Diagnostics;

namespace Test
{
    class Program
    {
        const int ArrLength = 100000;

        static void Main(string[] args) {
            tryRawData();

            Console.ReadKey();
        }

        static void tryRawData() {

            Stopwatch sp = new Stopwatch();

            sp.Start();

            StdfParse dataParse = new StdfParse(@"E:\Data\CP1-CP-FPA105.1-PTD211I-63K6M956.1-FPA105-21F7-20190721151225.stdf");
            dataParse.ExtractStdf();

            Console.WriteLine(sp.ElapsedMilliseconds);
            sp.Stop();
            sp.Reset();

            //Console.WriteLine(rawData.GetItemData(0).Length);


        }



    }
}