using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Animated Object")]
    [Serializable]
    public class AnimatedObjectBody : Body
    {
        #region construction
        public AnimatedObjectBody(Creature creature, Material bodyMaterial, AnimatedObject.BodyForm form)
            : base(Size.Tiny, bodyMaterial, false, 0, form != AnimatedObject.BodyForm.BiPedal)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.SlamSlot));
            _BodyForm = form;

            var _landMove = new LandMovement(BaseLandSpeed(Size.Tiny), creature, this);
            Movements.Add(_landMove);
            if (!(bodyMaterial is EarthMaterial)
                && !(bodyMaterial is MetalMaterial))
            {
                // any non-earth/metal will float (probably should burn this decision into the material!)
                var _halfSpeed = HalfSpeed(Size.Tiny);
                Movements.Add(new SwimMovement(_halfSpeed, creature, this, false, null));
                if (_BodyForm == AnimatedObject.BodyForm.Coilable)
                {
                    Movements.Add(new ClimbMovement(_halfSpeed, creature, this, false, null));
                }
                else if (_BodyForm == AnimatedObject.BodyForm.Sheet)
                {
                    // TODO: consider limits to size for flight
                    Movements.Add(new FlightSuMovement(_halfSpeed, creature, this, FlightManeuverability.Clumsy, false, false));
                }
            }
        }
        #endregion

        #region private data
        private AnimatedObject.BodyForm _BodyForm;
        #endregion

        public AnimatedObject.BodyForm BodyForm { get { return _BodyForm; } }

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: trample action (as trait?)
            // TODO: blind action (as trait?)
            // TODO: constrict action (as trait?)
            return base.GetActions(budget);
        }

        #region private int BaseLandSpeed(Size size)
        private int BaseLandSpeed(Size size)
        {
            var _extra =
                (_BodyForm == AnimatedObject.BodyForm.BiPedal) ? 10 :
                (_BodyForm == AnimatedObject.BodyForm.QuadraPedal) ? 20 :
                (_BodyForm == AnimatedObject.BodyForm.Roller) ? 40 :
                0;
            switch (size.Order)
            {
                case -1: return 30 + _extra;
                case 0: return 30 + _extra;
                case 1: return 20 + _extra;
                case 2: return 20 + _extra;
                case 3: return 10 + _extra;
                case 4: return 10 + _extra;
                default: return 40 + _extra;
            }
        }
        #endregion

        #region private int HalfSpeed(Size size)
        private int HalfSpeed(Size size)
        {
            // TODO: consider aquatic objects having superior swim and horrible land
            switch (size.Order)
            {
                case -1: return 15;
                case 0: return 15;
                case 1: return 10;
                case 2: return 10;
                case 3: return 5;
                case 4: return 5;
                default: return 20;
            }
        }
        #endregion

        #region protected override void OnSizeChanged(Size oldValue, Size newValue)
        protected override void OnSizeChanged(Size oldValue, Size newValue)
        {
            // speed adjustments
            foreach (var _move in Movements)
            {
                if (_move is LandMovement)
                {
                    _move.BaseValue = BaseLandSpeed(newValue);
                }
                else
                {
                    _move.BaseValue = HalfSpeed(newValue);
                }
            }
            base.OnSizeChanged(oldValue, newValue);
        }
        #endregion

        protected override Body InternalClone(Material material)
        {
            var _body = new AnimatedObjectBody(Creature, material, BodyForm);

            // NOTE: default constructor adds movements, we strip them to match calling conventions
            _body.Movements.Clear();
            return _body;
        }

        public override bool HasBones
        {
            get { return false; }
        }

        public override bool HasAnatomy
        {
            get { return false; }
        }
    }
}
