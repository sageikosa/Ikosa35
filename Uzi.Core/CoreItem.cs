using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreItem : CoreObject, ICoreItem
    {
        public CoreItem(string name)
            : base(name)
        {
            _OriginalName = name;
        }

        #region state
        private readonly string _OriginalName;
        protected CoreActor _Possessor = null;
        #endregion

        public string OriginalName => _OriginalName;

        /// <summary>Override to do something before Possessor changes</summary>
        protected virtual void OnPrePossessorChanged(CoreActor incomingPossessor) { }

        /// <summary>Override to do something after Possessor changes</summary>
        protected virtual void OnPossessorChanged() { }

        #region public CoreActor Possessor
        /// <summary>Pointer to the possessor</summary>
        public CoreActor Possessor
        {
            get => _Possessor;
            set
            {
                if (_Possessor != value)
                {
                    OnPrePossessorChanged(value);
                    _Possessor?.Possessions.RemovedFrom(this);
                    _Possessor = value;
                    _Possessor?.Possessions.Add(this);
                    OnPossessorChanged();
                    DoPropertyChanged(nameof(Possessor));
                }
            }
        }
        #endregion

        public override bool IsTargetable => true;

        public override string GetKnownName(CoreActor actor)
            => GetInfoData.GetInfoFeedback(this, actor)?.Message
            ?? OriginalName;
    }
}
