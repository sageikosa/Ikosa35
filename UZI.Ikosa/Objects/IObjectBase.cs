using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;
using System.Linq;

namespace Uzi.Ikosa.Objects
{
    public interface IObjectBase : ICoreObject, ITacticalInquiry, IMoveAlterer,
        IArmorRating, ISizable, IStructureDamage, IProvideSaves
    {
        Deltable ExtraSoundDifficulty { get; }
        int Hardness { get; }
        bool Masterwork { get; }
        int MaxStructurePoints { get; }
        Uzi.Ikosa.Items.Materials.Material ObjectMaterial { get; set; }
        ObjectSizer ObjectSizer { get; }

        /// <summary>True if this object can sit in a locator by itself</summary>
        bool IsLocatable { get; }
    }

    public static class IObjectBaseHelper
    {
        /// <summary>true if no locator</summary>
        public static bool InLocator(this ICoreObject self, MovementTacticalInfo moveTactical)
        {
            // default is an exact locator match
            var _rgn = self.GetLocated()?.Locator.GeometricRegion;
            return ((moveTactical.SourceCell != null) && (_rgn?.ContainsCell(moveTactical.SourceCell) ?? true))
                || ((moveTactical.TargetCell != null) && (_rgn?.ContainsCell(moveTactical.TargetCell) ?? true));
        }

        /// <summary>true if no locator</summary>
        public static bool InLocator(this ICoreObject self, IGeometricRegion region)
        {
            // default is an exact locator match
            var _rgn = self.GetLocated()?.Locator.GeometricRegion;
            return (_rgn?.ContainsGeometricRegion(region) ?? true);
        }

        /// <summary>Call this before unpathing and ungrouping so anchored objects still have a path to setting context</summary>
        public static void DoDestruction(this IObjectBase self)
        {
            if (self.StructurePoints > 0)
            {
                // this should call back into DoDestuction() with 0 structure points
                // but will also call UnPath() and UnGroup()
                self.StructurePoints = 0;
            }
            else
            {
                // anchored get to handle destruction
                if (self is IAnchorage _self)
                {
                    foreach (var _c in _self.Anchored.ToList())
                    {
                        var _destruct = new RootDestruction(self);
                        _c.HandleInteraction(new Interaction(null, self, _c, _destruct));
                    }
                }
            }
        }
    }
}
