using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class GrappleCheckStep : PreReqListStepBase
    {
        public GrappleCheckStep(CoreActivity activity, string doerDescription, string opposerDescription, Creature opposed = null)
            : base((CoreProcess)null)
        {
            if (activity.Actor is Creature _critter)
            {
                // creature check
                _PendingPreRequisites.Enqueue(new RollPrerequisite(this, new Qualifier(_critter, this, opposed), _critter,
                    $@"Doer.Opposed", doerDescription, new DieRoller(20), false));

                var _ox = 0;
                if (opposed == null)
                {
                    // opposed by all
                    foreach (var _opposed in _critter.Adjuncts.OfType<Grappler>()
                        .SelectMany(_g => _g.GrappleGroup.Grapplers)
                        .Select(_g => _g.Creature)
                        .Distinct().ToList())
                    {
                        _PendingPreRequisites.Enqueue(new RollPrerequisite(this, new Qualifier(_opposed, this, _critter), _opposed,
                            $@"Opposer.{_ox}", opposerDescription, new DieRoller(20), false));
                        _ox++;
                    }
                }
                else if (_critter.IsGrappling(opposed.ID))
                {
                    // opposed by just one
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, new Qualifier(opposed, this, _critter), opposed,
                        $@"Opposer.{_ox}", opposerDescription, new DieRoller(20), false));
                }
            }
        }

        public RollPrerequisite DoerRoll
            => AllPrerequisites<RollPrerequisite>(@"Doer.Opposed").FirstOrDefault();

        protected override bool OnDoStep()
        {
            if (IsComplete)
                return true;

            // doer score (qualifiedValue, qualifiedDeltas, softQualified Delta?)
            var _doerRoll = DoerRoll;
            var _doerScore = _doerRoll.RollValue 
                + (_doerRoll.Qualification.Actor as Creature)?.OpposedDeltable.QualifiedValue(_doerRoll.Qualification);

            // TODO: best opposer score
            return true;
        }
    }
}
