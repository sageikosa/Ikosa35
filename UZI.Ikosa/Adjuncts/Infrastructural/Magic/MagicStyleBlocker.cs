using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicStyleBlocker<MStyle> : Adjunct, IInteractHandler
        where MStyle : MagicStyle
    {
        public MagicStyleBlocker(object source)
            : base(source)
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as CoreObject)?.AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
        {
            return new MagicStyleBlocker<MStyle>(Source);
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                var _data = workSet.InteractData as AddAdjunctData;
                if (_data != null)
                {
                    // cannot add
                    var _magic = _data.Adjunct as MagicPowerEffect;
                    if (_magic != null)
                    {
                        if (_magic.MagicStyle is MStyle)
                            workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
                    }
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }

        #endregion
    }
}
