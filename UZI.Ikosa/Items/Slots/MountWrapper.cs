using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class MountWrapper : SlottedItemBase, IMountWrapper
    {
        public MountWrapper(Creature possessor, IWieldMountable wieldable, string slotType)
            : base(wieldable.OriginalName, slotType)
        {
            _Possessor = possessor;
            _Mounted = wieldable;
        }

        private IWieldMountable _Mounted;

        public IWieldMountable MountedObject => _Mounted;
        public override ICoreObject BaseObject => _Mounted;
        public override double Weight { get { return _Mounted.Weight; } set { base.Weight = 0; } }

        public override bool IsTransferrable
            => true;

        protected override void OnPossessorChanged()
        {
            if (_Mounted != null)
            {
                _Mounted.Possessor = Possessor;
            }
            base.OnPossessorChanged();
        }

        public override void HandleInteraction(Interaction interact)
        {
            // pass handle interaction onto the mounted item
            if (MountedObject != null)
            {
                MountedObject.HandleInteraction(interact);
            }
        }

        public override Info GetInfo(CoreActor actor, bool baseValues)
        {
            // get infos from base object
            return MountedObject?.GetInfo(actor, baseValues);
        }

        protected override void OnSetItemSlot()
        {
            CreaturePossessor.ObjectLoad.Add(this);
            BaseObject.AddAdjunct(new Attended(CreaturePossessor));
            BaseObject.AddAdjunct(new WieldMounted(MainSlot as MountSlot));
            base.OnSetItemSlot();
        }

        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            CreaturePossessor.ObjectLoad.Remove(this, null);
            BaseObject.Adjuncts.OfType<Attended>().FirstOrDefault()?.Eject();
            BaseObject.Adjuncts.OfType<WieldMounted>().FirstOrDefault()?.Eject();
            base.OnClearSlots(slotA, slotB);
        }

        #region IMountWrapper Members

        public void MountItem(IWieldMountable newItem)
        {
            // NOTE: no implementation
        }

        public void UnmountItem()
        {
            // vaporize the wrapper
            ClearSlots();
        }

        public IEnumerable<OptionAimValue<ISlottedItem>> GetMountables()
        {
            // NOTE: not used, mountable targets provided by MountSlot
            yield break;
        }

        #endregion

        public override IEnumerable<string> IconKeys
        {
            get
            {
                if (MountedObject != null)
                {
                    // provide mounted object
                    foreach (var _iKey in MountedObject.IconKeys)
                    {
                        yield return _iKey;
                    }
                }

                yield break;
            }
        }

        protected override string ClassIconKey => string.Empty;

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool UnslottingProvokes => false;
    }

    public interface IWieldMountable : ISlottedItem
    {
        IEnumerable<string> SlotTypes { get; }
        WieldTemplate WieldTemplate { get; }
    }

    /// <summary>Implemented by MountWrapper, but also by specialized scabbards that do not vaporize on unmounting</summary>
    public interface IMountWrapper
    {
        void MountItem(IWieldMountable newItem);
        void UnmountItem();
        IWieldMountable MountedObject { get; }
        IEnumerable<OptionAimValue<ISlottedItem>> GetMountables();
    }
}