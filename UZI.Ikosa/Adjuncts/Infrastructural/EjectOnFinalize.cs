using System;
using System.Collections.Generic;
using Uzi.Core;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class EjectOnFinalize : Adjunct, IFinalizeProcess
    {
        #region ctor()
        /// <summary>Ejects self and listed adjunct when the designated process finalizes</summary>
        public EjectOnFinalize(Adjunct ejectingAdjunct, CoreProcess process, bool serialIncrease)
            : base(new Adjunct[] { ejectingAdjunct })
        {
            _Process = process;
            _Serial = serialIncrease;
        }

        /// <summary>Ejects self and all listed adjuncts when the designated process finalizes</summary>
        public EjectOnFinalize(Adjunct[] ejectingAdjuncts, CoreProcess process, bool serialIncrease)
            : base(ejectingAdjuncts)
        {
            _Process = process;
            _Serial = serialIncrease;
        }
        #endregion

        #region state
        private CoreProcess _Process;
        private bool _Serial;
        #endregion

        public override bool IsProtected => true;
        public Adjunct[] EjectingAdjuncts => Source as Adjunct[];
        public CoreProcess Process => _Process;
        public bool IncreasesSerialState => _Serial;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            _Process?.AddFinalizer(this);

            // add all controlled adjuncts
            if (EjectingAdjuncts != null)
                foreach (var _adj in EjectingAdjuncts)
                {
                    if (!Anchor.Adjuncts.Contains(_adj))
                        Anchor.AddAdjunct(_adj);
                }
        }

        protected override void OnDeactivate(object source)
        {
            // eject all controlled adjuncts
            if (EjectingAdjuncts != null)
            {
                if (IncreasesSerialState)
                {
                    Anchor?.IncreaseSerialState();
                }
                foreach (var _adj in EjectingAdjuncts)
                {
                    _adj.Eject();
                }
            }
            base.OnDeactivate(source);
        }

        public void FinalizeProcess(CoreProcess process, bool deactivated)
            => Eject();

        public override object Clone() 
            => new EjectOnFinalize(EjectingAdjuncts, Process, IncreasesSerialState); 
    }
}
