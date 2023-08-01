using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using DataContainer;

namespace ReadTest {
    public struct PirType {
        public int PartIdx;
        public TestID LastTestUid;

        public PirType(int initIdx, TestID hash) {
            PartIdx = initIdx;
            LastTestUid = hash;
        }
    }

    public partial class StdV4Reader : IDisposable {
        const int BufferSize = 1024 * 1024 * 4;
        const int BufCnt = 16;

        private FileStream _stream;
        private int _recordCnt;
        private int _pirCnt;
        List<long> posPirRecord;
        List<int> partIdxAtposPirRecord;

        private string _path;
        private IDataCollect _dc;

        byte[][] BlockBuf = new byte[BufCnt][];
        bool[] bufState = new bool[BufCnt];
        private ConcurrentDictionary<int, int> _blockRedundant = new ConcurrentDictionary<int, int>(BufCnt, 500);
        private ConcurrentDictionary<int, int> _blockPartIdx = new ConcurrentDictionary<int, int>(BufCnt, 500);
        long fileLength = 0;


        List<PinMapRecord> listPinMaps = new List<PinMapRecord>();
        Dictionary<byte, TestID> _lastUidBySite = new Dictionary<byte, TestID>();

        public StdV4Reader(string path) {
            _path = path;
            for(int i=0; i< BlockBuf.Length; i++) {
                BlockBuf[i] = new byte[BufferSize*2];
                bufState[i] = false;
            }
        }
        public enum ReadStatus {
            Done,
            FileInvalid,
            Error
        }
        public ReadStatus ReadRaw(IDataCollect dc) {
            _dc = dc;

            var s = new System.Diagnostics.Stopwatch();

            long validLength = -1;
            s.Start();

            //count pir(prr may not match with pir, but ignore here)
            //count record and get position of each record
            //check if this is a valid file
            //check if contain tsr and some other special record
            using (_stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize)) {
                if (!ValidFile()) return ReadStatus.FileInvalid;
                fileLength = _stream.Length;

                posPirRecord = new List<long>(100000);
                partIdxAtposPirRecord = new List<int>(100000);

                byte[] buf = new byte[4];
                ushort len = 0;
                byte[] dataBuf = new byte[ushort.MaxValue];

                long curPos = 0;
                byte lastBuf2 = 0;
                byte lastBuf3 = 0;
                while (_stream.Position < fileLength) {
                    curPos = _stream.Position;
                    if (!ReadRecordHeader(buf)) break;
                    len = BitConverter.ToUInt16(buf, 0);
                    if (buf[2] == 5 && buf[3] == 10) {
                        _pirCnt++;
                        if ((lastBuf2 == 5 && lastBuf3 == 20) || posPirRecord.Count==0) {
                            posPirRecord.Add(curPos);
                            partIdxAtposPirRecord.Add(_pirCnt - 1);
                        }
                    }
                    lastBuf2 = buf[2];
                    lastBuf3 = buf[3];
                    if (!ReadRecordData(dataBuf, len)) break;
                    _recordCnt++;
                    validLength = curPos;
                }
                
            }

            _dc.SetPirCount(_pirCnt);

            int blkIdx = 0;
            int lastBufIdx;
            lastBufIdx = -1;
            using (_stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize)) {

                while (_stream.Position <= validLength) {
                    var bufIdx = GetIdleBufIdx();
                    //Console.WriteLine("get:" + bufIdx);
                    int ll = _stream.Read(BlockBuf[bufIdx], 0, BufferSize);
                    var cbIdx = blkIdx;
                    while (!_blockRedundant.TryAdd(cbIdx, -1)) ;
                    while (!_blockPartIdx.TryAdd(cbIdx, -1)) ;
                    var lbIdx = lastBufIdx;
                    lastBufIdx = bufIdx;
                    Task.Run(() => {
                        Parse(bufIdx, lbIdx, cbIdx, ll);
                    });
                    blkIdx++;
                }
                WaitFinsh();
            }

            s.Stop();

            Console.WriteLine("Read Raw:" + s.ElapsedMilliseconds);
            Console.WriteLine("Total Record:" + _recordCnt);
            Console.WriteLine("Total Pir:" + _pirCnt);
            Console.WriteLine("Total TouchDown:" + posPirRecord.Count);
            Console.WriteLine("Total Block:" + blkIdx);
            Console.WriteLine("File Length:" + fileLength);
            Console.WriteLine("Valid Length:" + validLength);
            return ReadStatus.Done;

        }

        void Parse(int idx, int lbIdx, int curBlockIdx, int len) {
            //System.Threading.Thread.Sleep(10);

            int offset, partIdx;

            var rcdIdx = FindTouchDownOffset( ((long)curBlockIdx) * BufferSize);
            if (rcdIdx >= 0) {
                if (curBlockIdx == 0) {
                    offset = 0;
                    _blockRedundant[curBlockIdx] = -2;
                } else {
                    offset = (int)(posPirRecord[rcdIdx] - ((long)curBlockIdx) * BufferSize);
                    for (int i = 0; i < offset; i++) {
                        BlockBuf[lbIdx][BufferSize + i] = BlockBuf[idx][i];
                    }
                    _blockRedundant[curBlockIdx] = offset;
                }
                partIdx = partIdxAtposPirRecord[rcdIdx];
                _blockPartIdx[curBlockIdx] = partIdx;

            } else {
                if (curBlockIdx == 0) {
                    offset = 0;
                    _blockRedundant[curBlockIdx] = -2;
                    partIdx = 0;
                    _blockPartIdx[curBlockIdx] = partIdx;
                } else {
                    offset = FindRecordOffset(idx, len);
                    for (int i = 0; i < offset; i++) {
                        BlockBuf[lbIdx][BufferSize + i] = BlockBuf[idx][i];
                    }
                    _blockRedundant[curBlockIdx] = offset;

                    while(_blockPartIdx[curBlockIdx - 1] < 0) {
                        System.Threading.Thread.Sleep(2);
                    }
                    _blockPartIdx[curBlockIdx] = _blockPartIdx[curBlockIdx-1];
                    partIdx = _blockPartIdx[curBlockIdx];
                }
            }
            //Console.WriteLine("PirIdx:" + rcdIdx);


            //read....
            //System.Threading.Thread.Sleep(300);
            ushort ll;
            RecordType typ;
            byte[] dataBuf = new byte[ushort.MaxValue];
            PirType[] pirBuf = new PirType[255];
            while (true) {
                ll = BitConverter.ToUInt16(BlockBuf[idx], offset); 
                typ = new RecordType(BlockBuf[idx][offset + 2], BlockBuf[idx][offset + 3]);
                Array.Copy(BlockBuf[idx], offset + 4, dataBuf, 0, ll);
                //byte[] dataBuf = BlockBuf[idx].Skip(offset + 4).Take(ll).ToArray();
                offset = offset + ll + 4;
                if (offset > len) {
                    offset = offset - ll - 4;
                    break;
                } else {
                    AddRecord(typ, dataBuf, ll, ref pirBuf, ref partIdx);
                }
            }


            //read next block redundant data
            if ((long)(curBlockIdx + 1) * BufferSize < fileLength) {
                while (!_blockRedundant.ContainsKey(curBlockIdx + 1)) {
                    System.Threading.Thread.Sleep(2);
                }
                while (_blockRedundant[curBlockIdx + 1] < 0) {
                    System.Threading.Thread.Sleep(2);
                }

                int extLen = _blockRedundant[curBlockIdx + 1];
                len += extLen;

                //do read
                while (extLen > 0) {
                    ll = BitConverter.ToUInt16(BlockBuf[idx], offset);
                    typ = new RecordType(BlockBuf[idx][offset + 2], BlockBuf[idx][offset + 3]);
                    Array.Copy(BlockBuf[idx], offset + 4, dataBuf, 0, ll);
                    offset = offset + ll + 4;
                    if (offset > len) {
                        offset = offset - ll - 4;
                        break;
                    } else {
                        AddRecord(typ, dataBuf, ll, ref pirBuf, ref partIdx);
                    }
                }

                //release redundant data in next block
                _blockRedundant[curBlockIdx + 1] = -2;
            }

            //wait for data
            while (_blockRedundant[curBlockIdx] != -2) {
                System.Threading.Thread.Sleep(2);
            }

            bufState[idx] = false;
            //Console.WriteLine("Reset:" + idx);
            //Console.WriteLine("curBlkIdx:" + curBlockIdx);
        }

        //find idx in recordPosList which just larger or equal to the blkpos, means posPirRecord[i-1]<blkPos && posPirRecord[i]>=blkPos
        int FindTouchDownOffset(long blkPos) {
            for (int i = 0; i < posPirRecord.Count; i++) {
                if (posPirRecord[i] >= blkPos) {
                    if ((posPirRecord[i] - blkPos) < BufferSize) {
                        return i;
                    } else {
                        return -1;
                    }
                }
            }

            return -1;
        }

        int FindRecordOffset(int idx, int len) {
            if (len < 4) return len;

            for (int i = 0; i <= (len-4); i++) {
                var l = BitConverter.ToUInt16(BlockBuf[idx], i);
                if ((l + 4 + i + 4) <= len && RecordTypes.Contains(new RecordType(BlockBuf[idx][i + 2], BlockBuf[idx][i + 3])) && RecordTypes.Contains(new RecordType(BlockBuf[idx][l + 4 + i + 2], BlockBuf[idx][l + 4 + i + 3])) ) {
                    return i;
                } else if ((l + 4 + i)==len && RecordTypes.Contains(new RecordType(BlockBuf[idx][i + 2], BlockBuf[idx][i + 3])) ) {
                    return i; 
                }
            }

            return len;
        }

        void WaitFinsh() {
            while (true) {
                bool flg = false;
                for (int i = 0; i < BufCnt; i++) {
                    flg |= bufState[i];
                }
                if (flg)
                    System.Threading.Thread.Sleep(2);
                else
                    return;
            }
        }

        int GetIdleBufIdx() {
            while (true) {
                for (int i = 0; i < BufCnt; i++) {
                    if (!bufState[i]) {
                        bufState[i] = true;
                        //Console.WriteLine("GetBufIdx:" + i);
                        return i;
                    }
                }
                System.Threading.Thread.Sleep(2);
            }
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
            if (_stream.Read(far, 0, 6) < 6) {
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
            _path = null;
            _dc = null;
        }

        #region readRecord
        public static RecordType FAR = new RecordType(0, 10);
        public static RecordType ATR = new RecordType(0, 20);
        public static RecordType MIR = new RecordType(1, 10);
        public static RecordType MRR = new RecordType(1, 20);
        public static RecordType PCR = new RecordType(1, 30);
        public static RecordType HBR = new RecordType(1, 40);
        public static RecordType SBR = new RecordType(1, 50);
        public static RecordType PMR = new RecordType(1, 60);
        public static RecordType PGR = new RecordType(1, 62);
        public static RecordType PLR = new RecordType(1, 63);
        public static RecordType RDR = new RecordType(1, 70);
        public static RecordType SDR = new RecordType(1, 80);
        public static RecordType WIR = new RecordType(2, 10);
        public static RecordType WRR = new RecordType(2, 20);
        public static RecordType WCR = new RecordType(2, 30);
        public static RecordType PIR = new RecordType(5, 10);
        public static RecordType PRR = new RecordType(5, 20);
        public static RecordType TSR = new RecordType(10, 30);
        public static RecordType PTR = new RecordType(15, 10);
        public static RecordType MPR = new RecordType(15, 15);
        public static RecordType FTR = new RecordType(15, 20);
        public static RecordType BPS = new RecordType(20, 10);
        public static RecordType EPS = new RecordType(20, 20);
        public static RecordType GDR = new RecordType(50, 10);
        public static RecordType DTR = new RecordType(50, 30);

        public static HashSet<RecordType> RecordTypes = new HashSet<RecordType>(){
            new RecordType(0, 10), //FAR,
            new RecordType(0, 20), //ATR,
            new RecordType(1, 10), //MIR,
            new RecordType(1, 20), //MRR,
            new RecordType(1, 30), //PCR,
            new RecordType(1, 40), //HBR,
            new RecordType(1, 50), //SBR,
            new RecordType(1, 60), //PMR,
            new RecordType(1, 62), //PGR,
            new RecordType(1, 63), //PLR,
            new RecordType(1, 70), //RDR,
            new RecordType(1, 80), //SDR,
            new RecordType(2, 10), //WIR,
            new RecordType(2, 20), //WRR,
            new RecordType(2, 30), //WCR,
            new RecordType(5, 10), //PIR,
            new RecordType(5, 20), //PRR,
            new RecordType(10, 30),//TSR,
            new RecordType(15, 10),//PTR,
            new RecordType(15, 15),//MPR,
            new RecordType(15, 20),//FTR,
            new RecordType(20, 10),//BPS,
            new RecordType(20, 20),//EPS,
            new RecordType(50, 10),//GDR,
            new RecordType(50, 30) //DTR
        };

        public void AddRecord(RecordType recordType, byte[] recordData, ushort len, ref PirType[] pbuf, ref int pIdx) {
            if (recordType == PTR) AddPtr(recordData, len, pbuf);
            else if (recordType == FTR) AddFtr(recordData, len, pbuf);
            else if (recordType == MPR) AddMpr(recordData, len, pbuf);
            else if (recordType == PIR) AddPir(recordData, len, ref pbuf, pIdx++);
            else if (recordType == PRR) AddPrr(recordData, len);
            else if (recordType == FAR) AddFar(recordData, len);
            else if (recordType == ATR) AddAtr(recordData, len);
            else if (recordType == MIR) AddMir(recordData, len);
            else if (recordType == MRR) AddMrr(recordData, len);
            else if (recordType == PCR) AddPcr(recordData, len);
            else if (recordType == HBR) AddHbr(recordData, len);
            else if (recordType == SBR) AddSbr(recordData, len);
            else if (recordType == PMR) AddPmr(recordData, len);
            else if (recordType == PGR) AddPgr(recordData, len);
            else if (recordType == PLR) AddPlr(recordData, len);
            else if (recordType == RDR) AddRdr(recordData, len);
            else if (recordType == SDR) AddSdr(recordData, len);
            else if (recordType == WIR) AddWir(recordData, len);
            else if (recordType == WRR) AddWrr(recordData, len);
            else if (recordType == WCR) AddWcr(recordData, len);
            else if (recordType == TSR) AddTsr(recordData, len);
            else if (recordType == BPS) AddBps(recordData, len);
            else if (recordType == EPS) AddEps(recordData, len);
            else if (recordType == GDR) AddGdr(recordData, len);
            else if (recordType == DTR) AddDtr(recordData, len);
            //else throw new Exception("No matched record");

            _recordCnt++;
        }
        #endregion 


    }
}
