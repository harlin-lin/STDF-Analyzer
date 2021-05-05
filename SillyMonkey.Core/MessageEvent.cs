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

    public class Event_NewFilter : PubSubEvent<SubData> {

    }


}
