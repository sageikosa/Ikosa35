using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Centipede")]
    [Serializable]
    public class CentipedeBody : Body
    {
        #region construction
        public CentipedeBody(Creature creature)
            : base(Size.Tiny, HideMaterial.Static, true, 0, true)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Mouth));

            var _landMove = new LandMovement(BaseSpeed(Size.Tiny), creature, this);
            Movements.Add(_landMove);

            var _climbMove = new ClimbMovement(BaseSpeed(Size.Tiny), creature, this, true, null);
            Movements.Add(_climbMove);
        }
        #endregion

        #region private int BaseClimbSpeed(Size size)
        private int BaseSpeed(Size size)
        {
            switch (size.Order)
            {
                case -2:
                    return 20;
                case -1:
                    return 30;
                default:
                    return 40;
            }
        }
        #endregion

        #region protected override void OnSizeChanged(Size oldValue, Size newValue)
        protected override void OnSizeChanged(Size oldValue, Size newValue)
        {
            // speed adjustments
            foreach (var _move in Movements)
            {
                _move.BaseValue = BaseSpeed(newValue);
            }
            base.OnSizeChanged(oldValue, newValue);
        }
        #endregion

        protected override Body InternalClone(Material material)
        {
            var _body = new CentipedeBody(Creature);
            _body.Sizer.NaturalSize = Sizer.NaturalSize; // TODO: Size?

            // NOTE: default constructor adds movements, we strip them to match calling conventions
            _body.Movements.Clear();
            return _body;
        }

        public override bool HasBones
            => false;

        public override bool HasAnatomy
            => true;
    }
}
