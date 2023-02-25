using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    public static class IkosaAdjunctStatics
    {
        public static ulong GetSerialState(this IAdjunctable self)
            => (self?.Setting as LocalMap)?.MapContext?.SerialState ?? 0;

        public static ulong IncreaseSerialState(this IAdjunctable self)
        {
            var _ctx = (self?.Setting as LocalMap)?.MapContext;
            if (_ctx != null)
            {
                _ctx.SerialState++;
                return _ctx.SerialState;
            }
            return 0;
        }
    }
}
