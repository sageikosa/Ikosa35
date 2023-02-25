using System;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Languages
{
    [Serializable]
    public abstract class Language
    {
        protected Language(object source)
        {
            Source = source;
        }

        public virtual string Name => GetType().Name;

        /// <summary>CanProject means can make speak, rather than just understand</summary>
        public virtual bool CanProject => true;

        public abstract string Alphabet { get; }
        public readonly object Source;
        public string SourceString => Source.SourceName();

        public virtual bool IsCompatible(Type type)
            => type.IsAssignableFrom(GetType());

        public virtual bool IsAlphabetCompatible(string alpha)
            => Alphabet.Equals(alpha);

        public abstract Language GetCopy(object source);
    }
}
