using StdfReader.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StdfReader {

    public delegate StdfRecord DelConverter(byte[] x, Endian endian);

    public partial class StdfFile {
        public static RecordType FAR = new RecordType(0, 10);
        public static RecordType ATR = new RecordType(0, 20, new DelConverter(Records.V4.Atr.Converter));
        public static RecordType MIR = new RecordType(1, 10, new DelConverter(Records.V4.Mir.Converter));
        public static RecordType MRR = new RecordType(1, 20, new DelConverter(Records.V4.Mrr.Converter));
        public static RecordType PCR = new RecordType(1, 30, new DelConverter(Records.V4.Pcr.Converter));
        public static RecordType HBR = new RecordType(1, 40, new DelConverter(Records.V4.Hbr.Converter));
        public static RecordType SBR = new RecordType(1, 50, new DelConverter(Records.V4.Sbr.Converter));
        public static RecordType PMR = new RecordType(1, 60, new DelConverter(Records.V4.Pmr.Converter));
        public static RecordType PGR = new RecordType(1, 62, new DelConverter(Records.V4.Pgr.Converter));
        public static RecordType PLR = new RecordType(1, 63, new DelConverter(Records.V4.Plr.Converter));
        public static RecordType RDR = new RecordType(1, 70, new DelConverter(Records.V4.Rdr.Converter));
        public static RecordType SDR = new RecordType(1, 80, new DelConverter(Records.V4.Sdr.Converter));
        public static RecordType WIR = new RecordType(2, 10, new DelConverter(Records.V4.Wir.Converter));
        public static RecordType WRR = new RecordType(2, 20, new DelConverter(Records.V4.Wrr.Converter));
        public static RecordType WCR = new RecordType(2, 30, new DelConverter(Records.V4.Wcr.Converter));
        public static RecordType PIR = new RecordType(5, 10, new DelConverter(Records.V4.Pir.Converter));
        public static RecordType PRR = new RecordType(5, 20, new DelConverter(Records.V4.Prr.Converter));
        public static RecordType TSR = new RecordType(10, 30, new DelConverter(Records.V4.Tsr.Converter));
        public static RecordType PTR = new RecordType(15, 10, new DelConverter(Records.V4.Ptr.Converter));
        public static RecordType MPR = new RecordType(15, 15, new DelConverter(Records.V4.Mpr.Converter));
        public static RecordType FTR = new RecordType(15, 20, new DelConverter(Records.V4.Ftr.Converter));
        public static RecordType BPS = new RecordType(20, 10, new DelConverter(Records.V4.Bps.Converter));
        public static RecordType EPS = new RecordType(20, 20, new DelConverter(Records.V4.Eps.Converter));
        public static RecordType GDR = new RecordType(50, 10, new DelConverter(Records.V4.Gdr.Converter));
        public static RecordType DTR = new RecordType(50, 30, new DelConverter(Records.V4.Dtr.Converter));

        public static RecordType START = new RecordType(99, 99);
        public static RecordType END = new RecordType(99, 98);
        public static RecordType ERROR = new RecordType(99, 97);
        public static RecordType SKIP = new RecordType(99, 96);



        IStdfStreamManager _StreamManager;
        RewindableByteStream _Stream;

        Endian _Endian = Endian.Unknown;
        /// <summary>
        /// Exposes the Endianness of the file.  Will be "uknown" until
        /// after parsing has begun.
        /// </summary>
        public Endian Endian { get { return _Endian; } }

        long? _ExpectedLength = null;
        public long? ExpectedLength { get { return _ExpectedLength; } }

        private bool _EnableCaching = true;
        /// <summary>
        /// Indicates whether caching is enabled. (default=true)
        /// This can only be set before parsing has begun.
        /// </summary>
        /// <remarks>
        /// Caching enables multiple queries without reparsing the file.
        /// Naturally, there is memory overhead associated with this.
        /// The default is to enable caching, which will suit the primary scenarios
        /// better.  There will be scenarios where caching is not desirable.
        /// </remarks>
        public bool EnableCaching {
            get { return _EnableCaching; }
            set { _EnableCaching = value; }
        }

        private bool _ThrowOnFormatError = true;
        /// <summary>
        /// Indicates whether the library should throw in the case of
        /// a format error. (default=true)
        /// This can only be set before parsing has begun.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This allows both tolerant and strict applications
        /// to be written using the same library.
        /// </para>
        /// <para>
        /// If set to false, a <see cref="Records.FormatErrorRecord"/> will be
        /// pushed through the stream instead of throwing an exception,
        /// and if possible, parsing will continue.
        /// However, at this time, most format exceptions are unrecoverable.
        /// In the future, the parser might be more intelligent in the way it
        /// handles issues.
        /// </para>
        /// </remarks>
        public bool ThrowOnFormatError {
            get { return _ThrowOnFormatError; }
            set { _ThrowOnFormatError = value; }
        }

        /// <summary>
        /// Constructs an StdfFile for the given path, suitable
        /// for parsing STDF V4 files.
        /// </summary>
        /// <param name="path">The path the the STDF file</param>
        public StdfFile(string path) {
            _StreamManager = new DefaultFileStreamManager(path);
        }
        /// <summary>
        /// Gets all the records in the file as a "stream" of StdfRecord object
        /// </summary>
        public IEnumerable<StdfRecord> GetRecords() {

            foreach (var record in InternalGetAllRecords()) {
                if (record.GetType() == typeof(StartOfStreamRecord)) {
                    var sosRecord = (StartOfStreamRecord)record;
                    sosRecord.FileName = _StreamManager.Name;
                    if (_Endian == Endian.Unknown) {
                        _Endian = sosRecord.Endian;
                    }
                    _ExpectedLength = sosRecord.ExpectedLength;
                }
                yield return record;
            }
        }


        IEnumerable<StdfRecord> InternalGetAllRecords() {
            //set this in case the last time we ended in seek mode
            using (IStdfStreamScope streamScope = _StreamManager.GetScope()) {
                try {
                    _Stream = new RewindableByteStream(streamScope.Stream);
                    //read the FAR to get endianness
                    var endian = Endian.Little;
                    var far = new byte[6];
                    if (_Stream.Read(far, 6) < 6) {
                        yield return new StartOfStreamRecord { Endian = Endian.Unknown, ExpectedLength = _Stream.Length };
                        yield return new FormatErrorRecord {
                            Message = Resources.FarReadError,
                            Recoverable = false
                        };
                        yield return new EndOfStreamRecord();
                        yield break;
                    }
                    endian = far[4] < 2 ? Endian.Big : Endian.Little;
                    var stdfVersion = far[5];
                    var length = (endian == Endian.Little ? far[0] : far[1]);
                    if (length != 2) {
                        yield return new StartOfStreamRecord { Endian = endian, ExpectedLength = _Stream.Length };
                        yield return new FormatErrorRecord {
                            Message = Resources.FarLengthError,
                            Recoverable = false
                        };
                        yield return new EndOfStreamRecord { Offset = 2 };
                        yield break;
                    }
                    //validate record type
                    if (far[2] != 0) {
                        yield return new StartOfStreamRecord { Endian = endian, ExpectedLength = _Stream.Length };
                        yield return new FormatErrorRecord {
                            Offset = 2,
                            Message = Resources.FarRecordTypeError,
                            Recoverable = false
                        };
                        yield return new EndOfStreamRecord { Offset = 6 };
                        yield break;
                    }
                    //validate record type
                    if (far[3] != 10) {
                        yield return new StartOfStreamRecord { Endian = endian, ExpectedLength = _Stream.Length };
                        yield return new FormatErrorRecord {
                            Offset = 3,
                            Message = Resources.FarRecordSubTypeError,
                            Recoverable = false
                        };
                        yield return new EndOfStreamRecord { Offset = 3 };
                        yield break;
                    }


                    //OK we're satisfied, let's go
                    yield return new StartOfStreamRecord() { Endian = endian, ExpectedLength = _Stream.Length };
                    yield return new StdfReader.Records.V4.Far() { CpuType = far[4], StdfVersion = far[5] };

                    //flush the memory
                    _Stream.Flush();

                    //now we have the FAR out of the way, and we can blow through the rest.
                    while (true) {
                        var position = _Stream.Offset;
                        //read a record header
                        RecordHeader? header = _Stream.ReadHeader(endian);
                        //null means we hit EOS
                        if (header == null) {
                            if (!_Stream.PastEndOfStream) {
                                //Something's wrong. We know the offset is rewound
                                //to the begining of the header.  If there's still
                                //data, we're corrupt
                                yield return new CorruptDataRecord() {
                                    Offset = position,
                                    //TODO: leverage the data in the stream.
                                    //we know we've hit the end, so we can just dump
                                    //the remaining memoized data
                                    CorruptData = _Stream.DumpRemainingData(),
                                    Recoverable = false
                                };
                                yield return new FormatErrorRecord() {
                                    Message = Resources.EOFInHeader,
                                    Recoverable = false,
                                    Offset = position
                                };
                            }
                            yield return new EndOfStreamRecord() { Offset = _Stream.Offset };
                            yield break;
                        }
                        var contents = new byte[header.Value.Length];
                        int read = _Stream.Read(contents, contents.Length);
                        if (read < contents.Length) {
                            //rewind to the beginning of the record (read bytes + the header)
                            _Stream.Rewind(_Stream.Offset - position);
                            yield return new CorruptDataRecord() {
                                Offset = position,
                                CorruptData = _Stream.DumpRemainingData(),
                                Recoverable = false
                            };
                            yield return new FormatErrorRecord() {
                                Message = Resources.EOFInRecordContent,
                                Recoverable = false,
                                Offset = position
                            };
                        }
                        else {
                            StdfRecord r;
                            if (header.Value.RecordType == PTR) r = PTR.Converter(contents, endian);
                            else if (header.Value.RecordType == MPR) r = MPR.Converter(contents, endian);
                            else if (header.Value.RecordType == FTR) r = FTR.Converter(contents, endian);
                            else if (header.Value.RecordType == ATR) r = ATR.Converter(contents, endian);
                            else if (header.Value.RecordType == MIR) r = MIR.Converter(contents, endian);
                            else if (header.Value.RecordType == MRR) r = MRR.Converter(contents, endian);
                            else if (header.Value.RecordType == PCR) r = PCR.Converter(contents, endian);
                            else if (header.Value.RecordType == HBR) r = HBR.Converter(contents, endian);
                            else if (header.Value.RecordType == SBR) r = SBR.Converter(contents, endian);
                            else if (header.Value.RecordType == PMR) r = PMR.Converter(contents, endian);
                            else if (header.Value.RecordType == PGR) r = PGR.Converter(contents, endian);
                            else if (header.Value.RecordType == PLR) r = PLR.Converter(contents, endian);
                            else if (header.Value.RecordType == RDR) r = RDR.Converter(contents, endian);
                            else if (header.Value.RecordType == SDR) r = SDR.Converter(contents, endian);
                            else if (header.Value.RecordType == WIR) r = WIR.Converter(contents, endian);
                            else if (header.Value.RecordType == WRR) r = WRR.Converter(contents, endian);
                            else if (header.Value.RecordType == WCR) r = WCR.Converter(contents, endian);
                            else if (header.Value.RecordType == PIR) r = PIR.Converter(contents, endian);
                            else if (header.Value.RecordType == PRR) r = PRR.Converter(contents, endian);
                            else if (header.Value.RecordType == TSR) r = TSR.Converter(contents, endian);
                            else if (header.Value.RecordType == BPS) r = BPS.Converter(contents, endian);
                            else if (header.Value.RecordType == EPS) r = EPS.Converter(contents, endian);
                            else if (header.Value.RecordType == GDR) r = GDR.Converter(contents, endian);
                            else if (header.Value.RecordType == DTR) r = DTR.Converter(contents, endian);
                            else r = new FormatErrorRecord();

                            //StdfRecord r = header.Value.RecordType.Converter(contents, endian);
                            if (r.GetType() != typeof(UnknownRecord)) {
                                //it converted, so update our last known position
                                //TODO: We should think about:
                                //* how to indicate corruption within the record boundaries
                                //* enabling filteres to set the last known offset (to allow valid unknown records to pass through)
                                //  * This could possible be done by allowing filters access to Flush or the dump functionality.
                                _Stream.Flush();
                            }
                            r.Offset = position;
                            yield return r;
                        }
                    }
                }
                finally {
                    //set stream to null so we're not holding onto it
                    _Stream = null;
                }
            }
        }

    }
}
