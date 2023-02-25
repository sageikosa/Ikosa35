using System;

namespace Uzi.Core
{
    [Serializable]
    public class QualifyingDelta : IDelta
    {
        #region ctor()
        public QualifyingDelta(int modifier, object source)
        {
            _Val = modifier;
            _Src = source;
            _Name = source.SourceName();
        }

        public QualifyingDelta(int modifier, object source, string name)
        {
            _Val = modifier;
            _Src = source;
            _Name = name;
        }
        #endregion

        #region data
        protected object _Src;
        protected string _Name;
        protected int _Val;
        #endregion

        public int Value => _Val;
        public object Source => _Src;
        public string Name => _Name;

        public bool Enabled { get => true; set { } }

        public static implicit operator int(QualifyingDelta mod)
            => mod.Value;

        #region Display Formatting
        public static string FormatModifier(IDelta mod)
            => FormatModifier(mod.Value, mod.Name);

        public static string FormatModifier(int val, string name)
            => (val >= 0)
                ? $@"+{val} from {name}"
                : $@"{val} from {name}";

        public override string ToString()
            => FormatModifier(_Val, Name);
        #endregion
    }
}
