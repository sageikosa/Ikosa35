using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public abstract class ModuleElement : IModuleElement
    {
        private readonly Guid _ID;
        private readonly Description _Description;

        /// <summary>Handles Description and post-deserialization Portfolio hierarchy</summary>
        protected ModuleElement(Description description)
        {
            _ID = Guid.NewGuid();
            _Description = description;
        }

        public virtual Guid ID => _ID;
        public Description Description => _Description;
    }
}
