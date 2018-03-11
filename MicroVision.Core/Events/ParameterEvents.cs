using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace MicroVision.Core.Events
{
    public class LoadEvent : PubSubEvent <string> { }
    public class SaveEvent : PubSubEvent { }

    public class SaveAsEvent : PubSubEvent<string> { }
    public class CaptureParameterChangedEvent : PubSubEvent { }
    public class LaserParmaeterChangedEvent : PubSubEvent { }
}
