using System.Linq;

namespace Uzi.Core
{
    /// <summary>Maintains an AdjunctSet</summary>
    public interface IAdjunctSet : ICore
    {
        AdjunctSet Adjuncts { get; }
    }

    /// <summary>Manages Adjuncts.</summary>
    public interface IAdjunctable : IAdjunctSet
    {
        CoreSetting Setting { get; }
        bool AddAdjunct(Adjunct adjunct);
        bool RemoveAdjunct(Adjunct adjunct);
    }

    public static class AdjunctStatics
    {
        public static bool HasAdjunct<Adj>(this IAdjunctSet self) where Adj : Adjunct
            => self.Adjuncts.OfType<Adj>().Any();

        public static bool HasActiveAdjunct<Adj>(this IAdjunctSet self, Adj exclude = null) where Adj : Adjunct
            => self.Adjuncts.OfType<Adj>().Any(_adj => _adj.IsActive && (_adj != exclude));
    }
}
