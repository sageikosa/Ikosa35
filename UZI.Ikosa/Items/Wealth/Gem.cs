using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core.Dice;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Wealth
{
    [Serializable]
    public class Gem : ItemBase
    {
        private enum GemColor : byte
        {
            Black, Blue, LimeGreen, Magenta, Orange, Turquoise, White, Yellow, Cyan, Violet, Red, Salmon
        }

        public Gem(string realName, string trueName, decimal price, Size size, double weight, int maxStruc, GemMaterial material)
            : base(trueName ?? material.Name ?? realName, size)
        {
            Name = realName;
            ItemSizer.NaturalSize = size;
            BaseWeight = weight;
            MaxStructurePoints.BaseValue = maxStruc;
            Price.IsTradeGood = true;
            Price.CorePrice = price;
            ItemMaterial = material;
            if (!string.IsNullOrWhiteSpace(material.AccentColor))
            {
                AddAdjunct(new ColorMapAdjunct { ColorMap = { { @"Material", material.AccentColor } } });
            }
            else
            {
                AddAdjunct(new ColorMapAdjunct { ColorMap = { { @"Material", ((GemColor)(DieRoller.RollDie(Guid.Empty, 12, @"Gem", @"Icon color") - 1)).ToString() } } });
            }
        }

        protected override string ClassIconKey
            => ((ItemMaterial as GemMaterial)?.IsEdged ?? false)
            ? @"edged_gem"
            : @"gem";

        public override IEnumerable<string> IconKeys
        {
            get
            {
                // all adjunct overrides
                foreach (var _ik in base.IconKeys.Where(_ik => _ik != ClassIconKey))
                {
                    yield return _ik;
                }

                // standard class Icon
                yield return ClassIconKey;
                yield break;
            }
        }
    }
}
