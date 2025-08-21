using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Skills
{
    /// <summary>WIS; untrained</summary>
    [Serializable, SkillInfo("Listen", MnemonicCode.Wis)]
    public class ListenSkill : SkillBase
    {
        public ListenSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify, object baseSource, string baseName)
        {
            if ((qualify as Interaction)?.InteractData is SoundData _sound)
            {
                // listen gets a boost if listener is aware of the originator
                if (Creature.Awarenesses.GetAwarenessLevel(_sound.Audible.SourceID) >= Senses.AwarenessLevel.Aware)
                {
                    var _audible = (Creature.Setting as LocalMap)?.GetICore<ICore>(_sound.Audible.SourceID);
                    yield return new QualifyingDelta(4, _audible, @"Aware");
                }
            }
            foreach (var _del in base.QualifiedDeltas(qualify, baseSource, baseName))
            {
                yield return _del;
            }

            yield break;
        }
    }
}
