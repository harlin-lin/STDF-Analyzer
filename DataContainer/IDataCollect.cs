using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer{
    public interface IDataCollect: IDisposable{
        void SetBasicInfo(string name, string val);
        void AddSiteNum(byte siteNum);
        void AddPir(byte siteNum);
        void AddSbr(ushort binNO, Tuple<string, string> binNmaeInfo);
        void AddHbr(ushort binNO, Tuple<string, string> binNmaeInfo);
        void UpdateItemInfo(string uid, ItemInfo itemInfo);
        void AddTestData(byte siteNum, string uid, float rst);
        void AddPrr(byte siteNum, UInt32? testTime, UInt16 hardBin, UInt16 softBin, string partId,
            short? xCord, short? yCord, DeviceType deviceType, ResultType result);
        //TestID GetUid(TempID id, byte siteNum);
        //TestID GetUid_AutoIncSubId(TestID uid);
        ItemInfo IfContainItemInfo(string uid);
        void SetReadingPercent(int percent);
        void AnalyseData();
    }
}
