using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace MicroVision.Core.Events
{
    #region Emission
    public class VimbaConnectedEvent : PubSubEvent { }
    public class VimbaDisconnectedEvent : PubSubEvent { }
    public class VimbaFrameCapturedEvent : PubSubEvent { }
    public class VimbaFrameSavedEvent : PubSubEvent { }
    public class VimbaErrorOccuredEvent : PubSubEvent<string> { }

    #endregion
}
