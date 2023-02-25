using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// Something that has one or more IInfo
    /// </summary>
    public interface IInformable
    {
        IEnumerable<Info> Inform(CoreActor observer);
    }
}
