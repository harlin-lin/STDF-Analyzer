using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataInterface;

namespace SillyMonkey.Core
{
    public class Event_DataSelected : PubSubEvent<IDataAcquire> {
    
    }

    public class Event_SubDataSelected : PubSubEvent<SubData> {

    }

    public class Event_FilterUpdated : PubSubEvent<SubData> {

    }

    public class Event_OpenData : PubSubEvent<SubData> {

    }

    public class Event_CloseData : PubSubEvent<SubData> {

    }

    public class Event_OpenFile : PubSubEvent<string> {

    }
    public class Event_ParseFileDone : PubSubEvent<IDataAcquire> {

    }
    public class Event_CloseFile : PubSubEvent<string> {

    }

    public class Event_CloseAllFiles : PubSubEvent {

    }

    public class Event_CloseSillyMonkey : PubSubEvent {

    }

    public class Event_ShowTrend : PubSubEvent<int> {

    }


}
