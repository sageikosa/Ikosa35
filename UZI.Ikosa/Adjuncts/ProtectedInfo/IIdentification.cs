using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Used on adjuncts that provide additional info on non-base info detection</summary>
    public interface IIdentification
    {
        IEnumerable<Info> IdentificationInfos { get; }
    }
}
