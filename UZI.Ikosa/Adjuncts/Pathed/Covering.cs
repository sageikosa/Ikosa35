using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Covering : Pathed, ISlotPathed
    {
        public Covering(CoveringWrapper cover)
            : base(cover)
        {
            _Covering = false;
        }

        #region data
        private bool _Covering;
        #endregion

        public CoveringWrapper CoveringWrapper
            => Source as CoveringWrapper;

        /// <summary>Can be used to see is if a lingering Covering adjunct needs to be ejected.</summary>
        public bool IsCovering => _Covering;

        public override bool IsProtected => true;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            CoveringWrapper?.CoverSource?.ActivateCover(CoveringWrapper?.CreaturePossessor);
            _Covering = true;
        }

        protected override void OnDeactivate(object source)
        {
            // get who is covered with what
            var _critter = CoveringWrapper?.CreaturePossessor;
            var _coverSource = CoveringWrapper?.CoverSource;

            // no longer is covering
            _coverSource?.DeactivateCover(_critter);
            _Covering = false;

            // uncover (which clears slots and removes holder and slot)
            CoveringWrapper?.ClearSlots();

            // and drop if coverSource still has structure
            if ((_critter != null) && ((_coverSource?.StructurePoints ?? 0) > 0))
            {
                Drop.DoDropEject(_critter, _coverSource);
            }

            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Covering(CoveringWrapper);

        public override IAdjunctable GetPathParent()
            => CoveringWrapper;

        public override string GetPathPartString() => @".";

        public override void UnPath()
        {
            CoveringWrapper?.ClearSlots();
        }

        #region ISlotPathed Members

        public IEnumerable<ItemSlot> PathedSlots
            => CoveringWrapper.AllSlots;

        public ISlottedItem SlottedConnector
            => CoveringWrapper;

        #endregion
    }

    public interface ICanCover : IItemBase
    {
        void ActivateCover(ICoreObject coreObj);
        void DeactivateCover(ICoreObject coreObj);
    }

    public interface ICanCoverAsSlot : ICanCover
    {
        ActionTime CoverageSlottingTime { get; }
        ActionTime CoverageUnSlottingTime { get; }
        bool CoverageSlottingProvokes { get; }
        bool CoverageUnSlottingProvokes { get; }
    }
}
