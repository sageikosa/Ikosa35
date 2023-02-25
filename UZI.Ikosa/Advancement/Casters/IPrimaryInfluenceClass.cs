using System.Collections.Generic;
using Uzi.Ikosa.Fidelity;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>Indicates that the power class is a primary devotional influence class.  Such as a cleric.</summary>
    public interface IPrimaryInfluenceClass : IPowerClass
    {
        IEnumerable<Influence> Influences { get; }
    }
}
