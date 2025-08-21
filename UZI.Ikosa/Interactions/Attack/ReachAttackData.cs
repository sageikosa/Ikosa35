using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    public class ReachAttackData : AttackData
    {
        #region construction
        /// <summary>Ranged and reach attacks</summary>
        public ReachAttackData(CoreActor actor,  ActionBase action, Locator attackLocator, 
            AttackImpact impact, Deltable score, bool harmless, Point3D attackPoint,
            ICellLocation source, ICellLocation target, int targetIndex, int targetCount)
            : this(actor, action, attackLocator, impact, score, null, harmless, attackPoint, source, target, targetIndex, targetCount)
        {
        }

        /// <summary>Ranged and reach attacks</summary>
        public ReachAttackData(CoreActor actor,  ActionBase action, Locator attackLocator, 
            AttackImpact impact, Deltable score, Deltable criticalConfirm, bool harmless,
            Point3D attackPoint, ICellLocation source, ICellLocation target, int targetIndex, int targetCount)
            : base(actor, action, attackLocator, impact, score, criticalConfirm, harmless, source, target, targetIndex, targetCount)
        {
            _AtkPt = attackPoint;
        }
        #endregion

        #region data
        private Point3D _AtkPt;
        #endregion

        /// <summary>Point of origin for the attack</summary>
        public Point3D AttackPoint { get { return _AtkPt; } set { _AtkPt = value; } }

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (Attacker is Creature _critter)
            {
                foreach (var _delta in _critter.RangedDeltable.QualifiedDeltas(qualify, this, @"Reach"))
                {
                    yield return _delta;
                }
            }
            else
            {
                yield return new QualifyingDelta(0, GetType(), @"Melee");
            }
            yield break;
        }
    }
}