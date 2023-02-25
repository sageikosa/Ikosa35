using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    public interface IBurstCaptureCapable : ICapability
    {
        /// <summary>After the burst is setup, this is called to perform any post construction steps.</summary>
        void PostInitialize(BurstCapture burst);

        /// <summary>When the burst is activated, this is called for each locator within the burst</summary>
        IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator);

        /// <summary>After the locators are selected, this is called to determine the processing order.</summary>
        IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection);
    }
}
