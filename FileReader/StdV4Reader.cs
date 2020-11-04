using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileReader {
    public struct RecordAddr{
        private int _offset;
        private int _length;
        private RecordType _recordType;

        public RecordAddr(byte[] headBuf, int offset) {
            _offset = offset;
            _length = BitConverter.ToUInt16(headBuf, 0);
            _recordType = new RecordType(headBuf[2], headBuf[3]);
        }

        public int Offset { get { return _offset; } }
        public int Length { get { return _length; } }
        public RecordType RecordType { get { return _recordType; } }

    }

    public class StdV4Reader:IDisposable {
        const int BufferSize = 409600;

        private FileStream _stream;
        private int _recordCnt;
        private string _path;

        public StdV4Reader(string path) {
            _path = path;
        }

        public RawData ReadRaw() {

            int partCnt = EvaluateFile();

            RawData rawData = new RawData(partCnt);

            using (_stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize)) {
                if (!ValidFile()) return null;
                long length = _stream.Length;
                //long estimateCnt = length / 80;

                byte[] buf = new byte[4];
                ushort len = 0;
                byte[] dataBuf = new byte[ushort.MaxValue];
                while (_stream.Position < length) {
                    if (!ReadRecordHeader(buf)) return rawData;
                    len = BitConverter.ToUInt16(buf, 0);
                    if (!ReadRecordData(dataBuf, len)) return rawData;
                    RawData.AddRecord(ref rawData, new RecordType(buf[2], buf[3]), dataBuf, len);
                    _recordCnt++;
                }
            }

            return rawData;
        }

        private int EvaluateFile() {
            int pirCnt = 0;
            int prrCnt = 0;
            //uint rcdCnt = 0;

            //bool rst = true;

            //var t = new System.Diagnostics.Stopwatch();
            //t.Start();
            try {
                using (_stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize)) {
                    if (!ValidFile()) throw new Exception("File not valid");
                    long length = _stream.Length;

                    byte[] buf = new byte[4];
                    ushort len = 0;
                    byte[] dataBuf = new byte[ushort.MaxValue];
                    while (_stream.Position < length) {
                        //if (!ReadRecordHeader(buf)) throw new Exception("Read head error");
                        _stream.Read(buf, 0, 4);
                        len = BitConverter.ToUInt16(buf, 0);

                        //find PIR
                        if (buf[2] == 5) {
                            if (buf[3] == 10) pirCnt++;
                            if (buf[3] == 20) prrCnt++;
                        }
                        //rcdCnt++;

                        _stream.Read(dataBuf, 0, len);
                        //if (!ReadRecordData(dataBuf, len)) throw new Exception("Read record error");
                    }
                }
                if (pirCnt != prrCnt) throw new Exception("PIR PRR mismatch");
            } catch (Exception e){
                _stream.Close();
                //Console.WriteLine(e);
                //rst = false;
                throw e;
            }
            //Console.WriteLine("Evaluate file time:" + t.ElapsedMilliseconds);
            //t.Stop();

            Console.WriteLine("Part Count:" + pirCnt);

            return pirCnt;
        }

        private bool ReadRecordHeader(byte[] buf) {
            if (_stream.Read(buf, 0, 4) == 4) {
                return true;
            } else {
                return false;
            }

        }
        private bool ReadRecordData(byte[] buf, ushort length) {
            if (_stream.Read(buf, 0, length) == length) {
                return true;
            } else {
                return false;
            }

        }


        private bool ValidFile() {
            bool valid = true;
            var far = new byte[6];
            if (_stream.Read(far,0, 6) < 6) {
                valid = false;
            }
            var length = far[0];
            if (length != 2) {
                valid = false;
            }
            //validate record type
            if (far[2] != 0) {
                valid = false;
            }
            //validate record type
            if (far[3] != 10) {
                valid = false;
            }
            _stream.Position = 0;
            return valid;
        }

        public void Dispose() {
            ((IDisposable)_stream).Dispose();
        }
    }
}
