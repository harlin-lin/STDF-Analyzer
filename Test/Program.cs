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

        static void Main(string[] args) {
            tryRawData();

            Console.ReadKey();
        }

        static void tryRawData() {

            Stopwatch sp = new Stopwatch();

            sp.Start();

            StdfParse dataParse = new StdfParse(@"E:\Data\E3200-0101_S905L_FT1_T3NS03.00_AEO932N081-D001_R3_20190817_010216.stdf");
            dataParse.ExtractStdf();

            Console.WriteLine(sp.ElapsedMilliseconds);
            sp.Stop();
            sp.Reset();

            //Console.WriteLine(rawData.GetItemData(0).Length);


        }

    }
}