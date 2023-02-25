using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Used as a base for things that should be flagged when making decisions on creature action choices and behavior</summary>
    [Serializable]
    public abstract class PredispositionBase : Adjunct
    {
        protected PredispositionBase(object source)
            : base(source)
        {
        }

        public abstract string Description { get; }
    }
}
