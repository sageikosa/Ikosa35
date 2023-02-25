using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Tactical
{
    public interface ITacticalMap
    {
        double CurrentTime { get; }

        /// <summary>Gets time marking start of current day</summary>
        double StartOfDay { get; }

        /// <summary>Gets time marking end of current day (or start of next day)</summary>
        double EndOfDay { get; }
    }

    public static class ITacticalMapStatic
    {
        /// <summary>
        /// Gets current time if possible by using IAdjunctable.Setting as ITacticalMap
        /// </summary>
        public static double? GetCurrentTime(this IAdjunctable self)
            => (self.Setting as ITacticalMap)?.CurrentTime;

        public static void StartNewProcess(this IAdjunctable self, CoreStep step, string name)
            => self?.GetLocated()?.Locator.IkosaProcessManager.StartProcess(new CoreProcess(step, name));
    }
}
