using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Drawer : CloseableContainerObject, IMonitorChange<OpenStatus>, IProcessFeedback, IAudibleOpenable
    {
        public Drawer(string name, Material material, ContainerObject container, bool blocksLight, int openNumber)
            : base(name, material, container, blocksLight, openNumber)
        {
            OpenState = this.GetOpenStatus(null, this, 1);
        }

        #region IAudibleOpenable Members
        private string GetMaterialString()
            => $@"{ObjectMaterial.SoundQuality}";

        public SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"opening", 
                (0, @"scraping"),
                (5, $@"{GetMaterialString()} scraping"),
                (10, $@"{GetMaterialString()} drawer opening")),
                8, 105, serialState);

        public SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closing", 
                (0, @"scraping"),
                (5, $@"{GetMaterialString()} scraping"),
                (10, $@"{GetMaterialString()} drawer closing")),
                8, 105, serialState);

        public SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closed", 
                (0, @"thump"),
                (5, $@"{GetMaterialString()} thump"),
                (10, $@"{GetMaterialString()} drawer closed")),
                4, 120, serialState);

        public SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"blocked", 
                (0, @"rattling"),
                (10, $@"{GetMaterialString()} drawer blocked")),
                8, 105, serialState);
        #endregion

        #region IMonitorChange<OpenStatus>
        public void PreTestChange(object sender, AbortableChangeEventArgs<OpenStatus> args)
        {
            if (args.NewValue.Value < 1)
            {
                // if not bound, cannot close
                if (!this.HasAdjunct<CloseableContainerBinder>())
                {
                    args.DoAbort(@"Not Connected", this);
                }
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
        }
        #endregion

        #region IInteractHandler
        IEnumerable<Type> IInteractHandler.GetInteractionTypes()
        {
            yield return typeof(RemoveAdjunctData);
            yield return typeof(AddAdjunctData);
            yield break;
        }

        void IInteractHandler.HandleInteraction(Interaction workSet)
        {
        }

        bool IInteractHandler.LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }

        void IProcessFeedback.ProcessFeedback(Interaction workSet)
        {
            switch (workSet?.InteractData)
            {
                case RemoveAdjunctData _remove:
                    if (_remove.Adjunct is CloseableContainerBinder)
                    {
                        // if successfully removed, open drawer
                        if (workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_fb => _fb.Value))
                        {
                            OpenState = this.GetOpenStatus(null, this, 1);
                        }
                    }
                    break;

                case AddAdjunctData _add:
                    if (_add.Adjunct is CloseableContainerBinder)
                    {
                        // if successfully added, close drawer
                        if (workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_fb => _fb.Value))
                        {
                            OpenState = this.GetOpenStatus(null, this, 0);
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}
