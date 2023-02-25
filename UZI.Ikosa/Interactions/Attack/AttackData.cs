using System;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using System.Collections.Generic;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Interaction is an attack (either ranged or melee)</summary>
    [Serializable]
    public abstract class AttackData : InteractData, ISupplyQualifyDelta
    {
        #region Construction
        protected AttackData(CoreActor actor, ActionBase action, Locator locator,
            AttackImpact impact, Deltable score, Deltable criticalConfirm, bool harmless,
            ICellLocation source, ICellLocation target,
            int targetIndex, int targetCount)
            : base(actor)
        {
            _Actor = actor;
            _Action = action;
            _Impact = impact;
            _Score = score;
            _NonLethal = false;
            CriticalThreat = criticalConfirm != null;
            CriticalConfirmation = criticalConfirm;
            Harmless = harmless;
            _SourceCell = source;
            _TargetCell = target;
            AttackScore?.Deltas.Add(new SoftQualifiedDelta(this));
            CriticalConfirmation?.Deltas.Add(new SoftQualifiedDelta(this));
            _TargetIndex = targetIndex;
            _TargetCount = targetCount;
            _Locator = locator;
        }
        #endregion

        #region data
        private Locator _Locator;
        private ActionBase _Action;
        private CoreActor _Actor;
        private AttackImpact _Impact;
        private Deltable _Score;
        private bool _NonLethal;
        private ICellLocation _TargetCell;
        private ICellLocation _SourceCell;
        private int _TargetIndex;
        private int _TargetCount;
        private bool _InRange = true;
        #endregion

        /// <summary>May be null if no activity was used during construction, or it's a trap</summary>
        public CoreActor Attacker => _Actor;

        public ActionBase Action => _Action;
        public Locator AttackLocator => _Locator;

        /// <summary>Automatic hit and miss reporting (or null for requiring a comparison)</summary>
        public bool? IsHit
            => !InRange ? false
            : AttackScore.BaseValue == 20 ? true
            : ((AttackScore.BaseValue == 1) ? false
            : (bool?)null);

        public bool InRange { get => _InRange; set => _InRange = value; }

        public bool IsNonLethal { get => _NonLethal; set => _NonLethal = value; }

        public AttackImpact Impact { get => _Impact; set => _Impact = value; }

        /// <summary>BaseValue is the raw attack roll.  EffectiveValue is the modified attack roll.</summary>
        public Deltable AttackScore { get => _Score; set => _Score = value; }

        public ICellLocation SourceCell { get => _SourceCell; set => _SourceCell = value; }

        /// <summary>Target Cell (for Melee or Location Aim)</summary>
        public ICellLocation TargetCell { get => _TargetCell; set => _TargetCell = value; }

        public int TargetIndex => _TargetIndex;
        public int TargetCount => _TargetCount;

        /// <summary>True if the attack is really a harmless touch (such as a beneficial spell)</summary>
        public bool Harmless { get; set; }

        /// <summary>True if the roll is in the threat range</summary>
        public bool CriticalThreat { get; set; }

        /// <summary>Attack roll to confirm a criticalConfirm.  BaseValue is the raw attack roll.  EffectiveValue is the modified attack roll.</summary>
        public Deltable CriticalConfirmation { get; set; }

        // ISupplyQualifyDelta Members
        public abstract IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify);
    }
}
