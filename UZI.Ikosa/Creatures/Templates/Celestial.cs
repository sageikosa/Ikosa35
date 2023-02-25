using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Creatures.Templates
{
    [Serializable]
    public class Celestial : AlignedExtraPlanar
    {
        public Celestial()
            : base(typeof(Celestial))
        {
        }

        public override string ClassIconKey => $@"{ClassName}_class";
        public override string TemplateName => nameof(Celestial);

        public override object Clone()
            => new Celestial();

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
            yield return typeof(PlantType);
            yield return typeof(VerminType);
            yield break;
        }

        protected override IEnumerable<EnergyType> ResistedEnergies()
        {
            yield return EnergyType.Acid;
            yield return EnergyType.Cold;
            yield return EnergyType.Electric;
            yield break;
        }

        protected override bool IsAlignable(Alignment alignment)
            => alignment.Ethicality != GoodEvilAxis.Evil;

        protected override Alignment GetSmitingAlignment()
            => Alignment.NeutralEvil;

        protected override Alignment GetCreatureAlignment()
            => Alignment.NeutralGood;
    }
}
