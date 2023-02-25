using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Creatures.Templates
{
    [Serializable]
    public class Fiendish : AlignedExtraPlanar
    {
        public Fiendish()
            : base(typeof(Fiendish))
        {
        }

        public override string ClassIconKey => $@"{ClassName}_class";
        public override string TemplateName => nameof(Fiendish);

        public override object Clone()
            => new Fiendish();

        protected override IEnumerable<Type> AllowedCreatureTypes()
        {
            yield return typeof(AberrationType);
            yield return typeof(AnimalType);
            yield return typeof(DragonType);
            yield return typeof(FeyType);
            yield return typeof(GiantType);
            yield return typeof(HumanoidType);
            yield return typeof(MagicalBeastType);
            yield return typeof(MonstrousHumanoidType);
            yield return typeof(OozeType);
            yield return typeof(PlantType);
            yield return typeof(VerminType);
            yield break;
        }

        protected override IEnumerable<EnergyType> ResistedEnergies()
        {
            yield return EnergyType.Cold;
            yield return EnergyType.Fire;
            yield break;
        }

        protected override bool IsAlignable(Alignment alignment)
            => alignment.Ethicality != GoodEvilAxis.Good;

        protected override Alignment GetSmitingAlignment()
            => Alignment.NeutralGood;

        protected override Alignment GetCreatureAlignment()
            => Alignment.NeutralEvil;
    }
}
