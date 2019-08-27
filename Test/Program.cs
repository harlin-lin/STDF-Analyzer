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
            RawData rawData = new RawData();
            Random r = new Random();

            Stopwatch sp = new Stopwatch();

            sp.Start();

            rawData.AddItem();
            rawData.Set(0, 4096, (float)r.NextDouble());

            Console.WriteLine(sp.ElapsedMilliseconds);
            sp.Stop();
            sp.Reset();

            //Console.WriteLine(rawData.GetItemData(0).Length);


        }



    }
}