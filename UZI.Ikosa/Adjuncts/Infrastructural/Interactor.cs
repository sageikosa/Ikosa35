using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Interactor<Handler> : Adjunct
        where Handler : class, IInteractHandler, new()
    {
        public Interactor(object source)
            : base(source)
        {
            _Handler = new Handler();
        }

        #region data
        private Handler _Handler;
        #endregion

        private CoreObject CoreObj
            => Anchor as CoreObject;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            CoreObj?.AddIInteractHandler(_Handler);
        }

        protected override void OnDeactivate(object source)
        {
            CoreObj?.RemoveIInteractHandler(_Handler);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Interactor<Handler>(Source);
    }
}
