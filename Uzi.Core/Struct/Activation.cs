using System;

namespace Uzi.Core
{
    [Serializable]
    public readonly struct Activation : ISourcedObject
    {
        public Activation(object source, bool active)
        {
            _Act= active;
            _Src = source;
        }

        private readonly object _Src;
        private readonly bool _Act;

        public bool IsActive => _Act; 
        public object Source => _Src; 
    }
}
