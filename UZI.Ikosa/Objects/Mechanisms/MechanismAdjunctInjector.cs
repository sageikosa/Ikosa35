using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    /// <summary>When a mechanism is bound to an object, this adjunct will be injected onto the anchorage</summary>
    [Serializable]
    public class MechanismAdjunctInjector : Adjunct, IPathDependent
    {
        /// <summary>When a mechanism is bound to an object, this adjunct will be injected onto the anchorage</summary>
        public MechanismAdjunctInjector(Adjunct adjunct)
            : base(adjunct)
        {
        }

        public Adjunct Injection => Source as Adjunct;

        public override bool IsProtected => true;
        public override object Clone()
            => new MechanismAdjunctInjector(Injection.Clone() as Adjunct);

        public void PathChanged(Pathed source)
        {
            var _binding = Anchor?.Adjuncts.OfType<ObjectBound>().FirstOrDefault();

            // if injection not same as object binding, eject it
            if (Injection.Anchor != _binding?.Anchorage)
            {
                Injection?.Eject();
            }

            // if injection not connected
            if (Injection.Anchor == null)
            {
                // try to add it to anchorage
                _binding?.Anchorage.AddAdjunct(Injection);
            }
        }
    }
}
