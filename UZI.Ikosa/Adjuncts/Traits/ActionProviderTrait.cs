using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ActionProviderTrait : TraitEffect
    {
        public ActionProviderTrait(ITraitSource traitSource, IActionProvider actionProvider)
            : base(traitSource)
        {
            _Provider = actionProvider;
        }

        private IActionProvider _Provider;
        public IActionProvider ActionProvider => _Provider;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.Actions.Providers.Add(ActionProvider, ActionProvider);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.Actions.Providers.Remove(ActionProvider);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new ActionProviderTrait(TraitSource, _Provider);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new ActionProviderTrait(traitSource, _Provider);
    }
}
