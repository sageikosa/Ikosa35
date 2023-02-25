using System;

namespace Uzi.Core
{
    [Serializable]
    public readonly struct DeltaValue
    {
        public DeltaValue(int seed)
        {
            Value = seed;
        }

        public readonly int Value;
    }
}
