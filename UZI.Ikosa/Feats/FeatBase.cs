using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public abstract class FeatBase : CreatureBindBase, IRefreshable, ISourcedObject
    {
        protected FeatBase(object source, int powerLevel)
            : base()
        {
            _Src = source;
            PowerLevel = powerLevel;
        }

        #region data
        private Guid _ID = Guid.Empty;
        private bool _Enabled = false;
        private object _Src;
        private Collection<RequirementMonitor> _ReqMonitors = null;
        #endregion

        protected Guid CoreID => _ID;
        public bool IsActive => _Enabled;
        public object Source => _Src;

        public bool IgnorePreRequisite = false;
        public int PowerLevel { get; private set; }

        /// <summary>Typically the unique Guid for the Feat</summary>
        public virtual Guid PresenterID => CoreID;

        public static FeatInfoAttribute FeatInfo(Type featType)
        {
            return featType.GetCustomAttributes(typeof(FeatInfoAttribute), false)
                .OfType<FeatInfoAttribute>()
                .FirstOrDefault();
        }

        public static ParameterizedFeatInfoAttribute ParameterizedFeatInfo(Type featType)
        {
            return featType.GetCustomAttributes(typeof(ParameterizedFeatInfoAttribute), false)
                .OfType<ParameterizedFeatInfoAttribute>()
                .FirstOrDefault();
        }

        public FeatInfo ToFeatInfo()
            => new FeatInfo
            {
                FullName = GetType().FullName,
                Message = Name,
                PowerLevel = PowerLevel,
                IsBonusFeat = IsBonusFeat(null),
                PreRequisite = PreRequisite,
                Benefit = Benefit,
                ID = (this as ICore)?.ID ?? Guid.Empty
            };

        public static string FeatName(Type featType)
        {
            var _fInfo = FeatInfo(featType);
            if (_fInfo != null)
                return _fInfo.Name;
            return featType.Name;
        }

        public static bool IsSingleton(Type featType)
        {
            var _fInfo = FeatInfo(featType);
            if (_fInfo != null)
                return _fInfo.Singleton;
            return true;
        }

        public static bool IsFighterBonus(Type featType)
            => featType.GetCustomAttributes(typeof(FighterBonusFeatAttribute), false)
                .OfType<FighterBonusFeatAttribute>().Any();

        public virtual string Name
            => FeatBase.FeatName(GetType());

        #region public bool IsBonusFeat(object source)
        public bool IsBonusFeat(object source)
        {
            if (Creature != null)
            {
                // if the hit dice are tracking this feat, its not a bonus feat
                if ((PowerLevel > 0)
                    && (PowerLevel <= Creature.AdvancementLog.NumberPowerDice)
                    && (Creature.AdvancementLog[PowerLevel].Feat == this))
                    return false;
                else
                    return Source == source;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public IEnumerable<RequirementAttribute> GetRequirements()
            => GetType().GetCustomAttributes(typeof(RequirementAttribute), true)
                .OfType<RequirementAttribute>();

        #region public virtual string PreRequisite
        public virtual string PreRequisite
        {
            get
            {
                var _builder = new StringBuilder();
                foreach (var _req in GetRequirements())
                {
                    _builder.Append($"{(_builder.Length > 0 ? "\n" : string.Empty)}{_req.Description}");
                }
                if (_builder.Length == 0)
                    return "None";
                else
                    return _builder.ToString();
            }
        }
        #endregion

        public abstract string Benefit { get; }

        public override bool CanAdd(Creature testCreature)
            => MeetsPreRequisite(testCreature);

        #region public virtual bool MeetsPreRequisite(Creature creature)
        /// <summary>Checks whether the bound feat can be added</summary>
        public virtual bool MeetsPreRequisite(Creature creature)
        {
            if (IgnorePreRequisite)
                return true;

            if (FeatBase.IsSingleton(GetType()) && creature.Feats.Contains(GetType()))
                return false;

            return GetRequirements().All(_r => _r?.MeetsRequirement(creature, PowerLevel) ?? true);
        }
        #endregion

        #region public virtual bool MeetsRequirements { get; }
        /// <summary>Checks whether the bound feat can be used</summary>
        public virtual bool MeetsRequirements
        {
            get
            {
                if (Creature == null)
                    return false;
                if (IgnorePreRequisite)
                    return true;
                return GetRequirements().All(_r => _r?.MeetsRequirement(Creature) ?? true);
            }
        }
        #endregion

        #region public virtual bool MeetsRequirementsAtPowerLevel { get; }
        /// <summary>Checks whether the bound feat could be used when it was taken</summary>
        public virtual bool MeetsRequirementsAtPowerLevel
        {
            get
            {
                if (Creature == null)
                    return false;
                if (IgnorePreRequisite)
                    return true;
                return GetRequirements().All(_r => _r?.MeetsRequirement(Creature, PowerLevel) ?? true);
            }
        }
        #endregion

        public override string ToString()
            => Name;

        protected override void OnAdd()
        {
            _ID = Guid.NewGuid();
            _Creature.Feats.Add(this);
            Refresh();
            if (!IgnorePreRequisite)
            {
                foreach (var _req in GetRequirements())
                {
                    _ReqMonitors ??= new Collection<RequirementMonitor>();

                    var _mon = _req.CreateMonitor(this, Creature);
                    if (_mon != null)
                        _ReqMonitors.Add(_mon);
                }
            }
        }

        public void Refresh()
        {
            var _meets = MeetsRequirements;
            if (!_Enabled && _meets)
                OnActivate();
            else if (_Enabled && !_meets)
                OnDeactivate();
        }

        protected override void OnRemove()
        {
            if (_Enabled)
                OnDeactivate();
            _Creature.Feats.Remove(this);

            if (_ReqMonitors != null)
            {
                foreach (var _dyn in _ReqMonitors)
                {
                    _dyn.DoTerminate();
                }
                _ReqMonitors = null;
            }
        }

        protected virtual void OnActivate() { _Enabled = true; }
        protected virtual void OnDeactivate() { _Enabled = false; }
    }
}