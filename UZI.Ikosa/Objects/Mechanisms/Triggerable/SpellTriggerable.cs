using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SpellTriggerable : Mechanism, ITriggerable
    {
        public SpellTriggerable(string name, Material material, int seedDifficulty)
            : base(name, material, seedDifficulty)
        {
            AddAdjunct(new TrapPart(true));
        }

        #region data
        private PostTriggerState _PostState;
        #endregion

        public PostTriggerState PostTriggerState
        {
            get => _PostState;
            set
            {
                _PostState = value;
                DoPropertyChanged(nameof(PostTriggerState));
            }
        }

        public override IEnumerable<IActivatable> Dependents
        {
            get
            {
                yield break;
            }
        }

        protected override string ClassIconKey => nameof(SpellTriggerable);

        public void DoTrigger(IActivatableObject mechanism, IEnumerable<Locator> locators)
        {
            if (Activation.IsActive && !IsDisabled)
            {
                // TODO: trigger spell...
                DoPostTrigger();
            }
        }

        protected void DoPostTrigger()
        {
            switch (PostTriggerState)
            {
                case PostTriggerState.Destroy:
                    StructurePoints = 0;
                    break;

                case PostTriggerState.Damage:
                    // NOTE: implicit disable due to damage
                    StructurePoints = 1;
                    break;

                case PostTriggerState.Disable:
                    if (!this.HasAdjunct<DisabledObject>())
                        AddAdjunct(new DisabledObject());
                    break;

                case PostTriggerState.DeActivate:
                    // NOTE: no implicit re-activation action for a trigger
                    //       must attach an activation mechanism to re-enable the trigger
                    //       or the specific trigger must provide an action itself
                    Activation = new Activation(this, false);
                    break;

                default:
                    // includes AutoReset
                    break;
            }
        }
    }
}
