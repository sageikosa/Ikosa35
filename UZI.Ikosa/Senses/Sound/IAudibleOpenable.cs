using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Senses
{
    public interface IAudibleOpenable : IOpenable
    {
        SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState);
        SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState);
        SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState);
        SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState);
        SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState);
    }
}
