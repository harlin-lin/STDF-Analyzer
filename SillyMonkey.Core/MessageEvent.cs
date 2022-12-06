using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContainer;

namespace SillyMonkey.Core
{
    public class Event_DataSelected : PubSubEvent<string> {
    
    }

    public class Event_SubDataSelected : PubSubEvent<SubData> {

    }

    public class Event_SubDataTabSelected : PubSubEvent<SubData> {

    }


    public class Event_FilterUpdated : PubSubEvent<SubData> {

    }

    public class Event_CloseData : PubSubEvent<SubData> {

    }

    public class Event_OpenFile : PubSubEvent<string> {

    }
    public class Event_CloseFile : PubSubEvent<string> {

    }

    public class Event_CloseAllFiles : PubSubEvent {

    }

    public class Event_CloseSillyMonkey : PubSubEvent {

    }

    public class Event_ItemsSelected : PubSubEvent<Tuple<SubData, List<string>>> {

    }

    public class Event_Progress : PubSubEvent<Tuple<string, int>> {

    }
    public class Event_Log : PubSubEvent<string> {

    }
    public class Event_FileInfo : PubSubEvent<string> {}

    public class Event_MergeFiles : PubSubEvent<List<string>> {}

    public class Event_CorrData : PubSubEvent<IEnumerable<SubData>> { }

    public class Event_CorrItemSelected : PubSubEvent<Tuple<string, IEnumerable<SubData>>> {}

    public delegate void SubWindowReturnHandler(object obj);
}
