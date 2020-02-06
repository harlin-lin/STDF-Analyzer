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

            StdfParse dataParse = new StdfParse(@"C:\Users\Harlin\Documents\Projects\STDF\Data\aaa.stdf");
            dataParse.ExtractStdf();

            Console.WriteLine(sp.ElapsedMilliseconds);
            sp.Stop();
            sp.Reset();

            //sp.Start();

            //var filters = dataParse.GetAllFilter();

            //Console.WriteLine(filters.ElementAt(0));
            //FilterSetup filterSetup = new FilterSetup();
            //filterSetup.ifmaskDuplicateChips = true;
            //dataParse.SetFilter(filters.ElementAt(0).Key, filterSetup);

            //Console.WriteLine(sp.ElapsedMilliseconds);
            //sp.Stop();
            //sp.Reset();


            //Console.WriteLine(rawData.GetItemData(0).Length);


        }

    }
}