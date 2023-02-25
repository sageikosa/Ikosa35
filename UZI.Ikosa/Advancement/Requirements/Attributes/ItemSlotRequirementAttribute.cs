using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class ItemSlotRequirementAttribute : RequirementAttribute
    {
        public ItemSlotRequirementAttribute(string name, string description, string slotType, string subType, int minimumValue, int maximumValue)
        {
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            SlotType = slotType;
            SubType = subType;
            _Name = name;
            _Description = description;
        }

        private string _Name;
        private string _Description;

        public readonly int MinimumValue;
        public readonly int MaximumValue;
        public readonly string SlotType;
        public readonly string SubType;

        public override string Name { get { return _Name; } }
        public override string Description { get { return _Description; } }

        public override bool MeetsRequirement(Creature creature)
        {
            var _body = creature.Body;
            var _count = _body.ItemSlots.AllSlots
                .Count(_is => _is.SlotType.Equals(SlotType, StringComparison.OrdinalIgnoreCase)
                && (_is.Source == _body)
                && ((SubType == null) || (SubType == _is.SubType)));
            return ((MinimumValue <= _count) && (_count <= MaximumValue));
        }

        public override RequirementMonitor CreateMonitor(IRefreshable target, Creature owner)
        {
            return null;
            //return new ItemSlotBodyMonitor(this, target, owner);
        }
    }
}
