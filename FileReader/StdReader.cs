using DataContainer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileReader {
    public delegate void ExtractDoneEventHandler(StdReader data);

    public enum StdFileType {
        STD,
        STD_GZ
    }
    /// <summary>
    /// Used to indicate endian-ness
    /// </summary>
    public enum Endian {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Big endian
        /// </summary>
		Big,
        /// <summary>
        /// Little Endian
        /// </summary>
		Little,
    }

    public class StdReader : IDisposable {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }

        public void ExtractStdf() {
            var s = new System.Diagnostics.Stopwatch();
            using (StdV4Reader _v4Reader = new StdV4Reader(FilePath)) {
                var dc = StdDB.GetDataCollect(FilePath);
                try {
                    s.Start();
                    _v4Reader.ReadRaw(dc);
                    s.Stop();
                    Console.WriteLine("Read Raw:" + s.ElapsedMilliseconds);
                    s.Restart();
                    dc.AnalyseData();
                    s.Stop();
                    Console.WriteLine("Analyse:" + s.ElapsedMilliseconds);
                }
                catch {
                    //release table in data base
                    throw;
                }
            }

        }

        public void Dispose() {
            FilePath = null;
            FileName = null;
        }

        public StdReader(string path, StdFileType stdFileType) {
            FilePath = path;
            FileName = Path.GetFileName(path);
        }


    }
}
