using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Provides adjunct exposure information</summary>
    public interface IExposeProtectable
    {
        bool DoesExpose(Adjunct target, Creature actor);
    }

    public static class ExposeProtectableHelper
    {
        public static bool HasExposureTo(this Adjunct self, Creature critter)
            => self?.Anchor?.Adjuncts
            .OfType<IExposeProtectable>()
            .Any(_i => _i.DoesExpose(self, critter)) ?? false;
    }
}