using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace MicroVision.Core.Events
{
    #region Requests
    public class VimbaListUpdateRequestedEvent : PubSubEvent { }
    public class VimbaConnectionRequestedEvent : PubSubEvent { }
    public class VimbaDisconnectionRequestedEvent : PubSubEvent { }
    public class VimbaCaptureStartRequestedEvent : PubSubEvent { } 
    public class VimbaCaptureStopRequestedEvent : PubSubEvent { }
    #endregion

    #region Emission
    public class VimbaConnectedEvent : PubSubEvent { }
    public class VimbaDisconnectedEvent : PubSubEvent { }
    public class VimbaFrameCapturedEvent : PubSubEvent { }
    public class VimbaFrameSavedEvent : PubSubEvent { }
    public class VimbaErrorOccuredEvent : PubSubEvent<string> { }

    #endregion
}
