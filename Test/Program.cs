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
            //tryRawData();

            TR tr = new TR();

            tr.logCount();

            var b1 = tr.BinsPub;
            var b2 = tr.GetBinsPri();

            for(int i=0; i<10; i++) {
                b1.Add((UInt16)i, i);
                b2.Add((UInt16)i, i);
            }
            tr.logCount();

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

        class TR{
            private Dictionary<UInt16, int> _binsPri;
            public Dictionary<UInt16, int> BinsPub { get; private set; }

            public Dictionary<UInt16, int> GetBinsPri() {

                return new Dictionary<UInt16, int>(_binsPri);
            }

            public TR() {
                _binsPri = new Dictionary<ushort, int>();
                BinsPub = new Dictionary<ushort, int>();
            }


            public void logCount() {
                Console.WriteLine(_binsPri.Count);
                Console.WriteLine(BinsPub.Count);
            }

        }
    }
}