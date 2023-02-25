using Uzi.Core;
using Uzi.Ikosa.Actions;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Ranged attacks</summary>
    public class RangedAttackData : ReachAttackData
    {
        #region construction
        /// <summary>Ranged attacks</summary>
        public RangedAttackData(CoreActor actor, IRangedSource rangedSource, ActionBase action, Locator attackLocator,
            AttackImpact impact, Deltable score, bool harmless, Point3D attackPoint,
            ICellLocation source, ICellLocation target, int targetIndex, int targetCount)
            : this(actor, rangedSource, action, attackLocator, impact, score, null, harmless, attackPoint, source, target, targetIndex, targetCount)
        {
        }

        /// <summary>Ranged attacks</summary>
        public RangedAttackData(CoreActor actor, IRangedSource rangedSource,  ActionBase action, Locator attackLocator,
            AttackImpact impact, Deltable score, Deltable criticalConfirm, bool harmless, Point3D attackPoint,
            ICellLocation source, ICellLocation target, int targetIndex, int targetCount)
            : base(actor, action, attackLocator, impact, score, criticalConfirm, harmless, attackPoint, source, target, targetIndex, targetCount)
        {
            _RangedSource = rangedSource;
        }
        #endregion

        #region private data
        private IRangedSource _RangedSource;
        private bool _Volley = false;
        #endregion

        public IRangedSource RangedSource => _RangedSource;

        /// <summary>Manyshot uses this to probe for additional damage on extra ammunition hits</summary>
        public bool IsVolleyExtra { get { return _Volley; } set { _Volley = value; } }
    }
}