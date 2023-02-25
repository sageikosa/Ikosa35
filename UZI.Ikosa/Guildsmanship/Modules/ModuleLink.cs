using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    /// <summary>
    /// Base class for reference-like module nodes that can link to the element
    /// </summary>
    /// <typeparam name="INode"></typeparam>
    [Serializable]
    public abstract class ModuleLink<INode> : ModuleElement
        where INode : class, IModuleNode
    {
        private Guid _NodeID;
        private string _Module;   // information if targetID is no longer accessible...from when it was accessible

        protected ModuleLink(Description description)
            : base(description)
        {
            _NodeID = Guid.Empty;
            _Module = string.Empty;
        }

        public Guid LinkedNodeID => _NodeID;
        public string LinkedModule => _Module;

        public void SetTarget(Guid targetID, string targetModule)
        {
            _NodeID = targetID;
            _Module = targetModule;
        }
    }
}
