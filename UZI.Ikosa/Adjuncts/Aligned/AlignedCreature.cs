using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class AlignedCreature : Aligned, ICreatureBound
    {
        public AlignedCreature(Alignment alignment)
            : base(typeof(Aligned), alignment)
        {
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (Anchor != null)
            {
                // remove any conflicting
                foreach (var _alCrit in (from _ac in Anchor.Adjuncts.OfType<AlignedCreature>()
                                         where (_ac.Alignment.Ethicality != Alignment.Ethicality)
                                         || (_ac.Alignment.Orderliness != Alignment.Orderliness)
                                         select _ac).ToList())
                {
                    _alCrit.Eject();
                }
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }

        public Creature Creature => Anchor as Creature;
        public override bool IsProtected => true;

        public override AuraStrength AlignmentStrength
        {
            get
            {
                var _pd = Creature.AdvancementLog.NumberPowerDice;
                if (_pd <= 10)
                {
                    return AuraStrength.Faint;
                }

                if (_pd <= 25)
                {
                    return AuraStrength.Moderate;
                }

                if (_pd <= 50)
                {
                    return AuraStrength.Strong;
                }

                return AuraStrength.Overwhelming;
            }
        }

        public override int PowerLevel
            => Creature.AdvancementLog.NumberPowerDice;

        public override object Clone()
            => new AlignedCreature(Alignment);
    }
}
