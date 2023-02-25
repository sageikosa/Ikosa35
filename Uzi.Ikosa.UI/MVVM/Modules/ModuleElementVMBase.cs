using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public abstract class ModuleElementVMBase : ModuleManagementVMBase
    {
        private readonly IModuleElement _Element;
        private readonly DescriptionVM _Description;

        protected ModuleElementVMBase(IModuleElement moduleElement)
        {
            _Element = moduleElement;
            _Description = new DescriptionVM(moduleElement.Description);
        }

        protected IModuleElement Element => _Element;
        public DescriptionVM Description => _Description;

        public abstract Module IkosaModule { get; }
    }
}
