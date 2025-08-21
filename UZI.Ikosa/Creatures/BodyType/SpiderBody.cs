using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Spider")]
    [Serializable]
    public class SpiderBody : Body
    {
        #region construction
        public SpiderBody(Creature creature, SpiderForm form)
            : base(Size.Tiny, ExoskeletonMaterial.Static, true, 0, true)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Mouth));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Spinneret, false));
            _Form = form;

            var _landMove = new LandMovement(BaseLandSpeed(Size.Tiny), creature, this);
            Movements.Add(_landMove);

            var _climbMove = new ClimbMovement(BaseClimbSpeed(Size.Tiny), creature, this, true, null);
            Movements.Add(_climbMove);
        }
        #endregion

        #region data
        private SpiderForm _Form;
        private Delta[] _Deltas;
        private IQualifyDelta[] _Qualifies;
        #endregion

        public SpiderForm SpiderForm => _Form;

        protected override void OnConnectBody()
        {
            base.OnConnectBody();

            // TODO: anchored strand descent

            if (SpiderForm == SpiderForm.Hunter)
            {
                // hunter skill boosts
                var _jump = new Delta(10, typeof(Racial), @"Spider Jump Bonus");
                Creature.Skills.Skill<JumpSkill>().Deltas.Add(_jump);

                var _spot = new Delta(8, typeof(Racial), @"Spider Spot Bonus");
                Creature.Skills.Skill<SpotSkill>().Deltas.Add(_spot);

                _Deltas = new Delta[] { _jump, _spot };
                _Qualifies = new IQualifyDelta[] { };
            }
            else
            {
                // web-spinner skill boosts and attacks
                // TODO: stealth +8 (webs), silent stealth +8 (webs)
                // TODO: move through own webs

                _Deltas = new Delta[] { };
                _Qualifies = new IQualifyDelta[] { };
            }
        }

        protected override void OnDisconnectBody()
        {
            // remove any deltas
            if (_Deltas != null)
            {
                foreach (var _d in _Deltas)
                {
                    _d.DoTerminate();
                }
            }

            // remove any IQualifyDeltas
            if (_Qualifies != null)
            {
                foreach (var _q in _Qualifies)
                {
                    _q.DoTerminate();
                }
            }

            base.OnDisconnectBody();
        }

        #region private int BaseLandSpeed(Size size)
        private int BaseLandSpeed(Size size)
        {
            var _extra = (_Form == SpiderForm.Hunter) ? 10 : 0;
            return BaseClimbSpeed(size) + 10 + _extra;
        }
        #endregion

        #region private int BaseClimbSpeed(Size size)
        private int BaseClimbSpeed(Size size)
        {
            switch (size.Order)
            {
                case -2:
                    return 10;
                default:
                    return 20;
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
                    _move.BaseValue = BaseClimbSpeed(newValue);
                }
            }
            base.OnSizeChanged(oldValue, newValue);
        }
        #endregion

        protected override Body InternalClone(Material material)
        {
            var _body = new SpiderBody(Creature, SpiderForm);
            _body.Sizer.NaturalSize = Sizer.NaturalSize; // TODO: Size?

            // NOTE: default constructor adds movements, we strip them to match calling conventions
            _body.Movements.Clear();
            return _body;
        }

        public override bool HasBones => false;
        public override bool HasAnatomy => true;
    }
}
