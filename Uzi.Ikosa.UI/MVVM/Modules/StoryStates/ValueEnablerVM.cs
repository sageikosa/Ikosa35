using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public abstract class ValueEnablerVM : ModuleElementVMBase
    {
        private readonly ValueEnablersFolderVM _Folder;

        protected ValueEnablerVM(ValueEnablersFolderVM folder, ValueEnabler enabler)
            : base(enabler)
        {
            _Folder = folder;
        }

        public ValueEnablersFolderVM Folder => _Folder;
        public ValueEnabler Enabler => Element as ValueEnabler;

        public override Module IkosaModule
            => Folder.VariableValue.Folder.Variable.Folder.IkosaModule;

        public string Name
        {
            get => Enabler.Name;
            set
            {
                Enabler.Description.Message = value;
                DoPropertyChanged(nameof(Name));
            }
        }

        public static ValueEnablerVM FromPreCondition(ValueEnablersFolderVM folder, ValueEnabler enabler)
        {
            switch (enabler)
            {
                case ItemValueEnabler _itemReq:
                    return new ItemValueEnablerVM(folder, _itemReq);

                case VariableValueEnabler _storyReq:
                    return new VariableValueEnablerVM(folder, _storyReq);

                case InfoKeyValueEnabler _infoReq:
                    return new InfoKeyValueEnablerVM(folder, _infoReq);

                default:
                    return null;
            }
        }
    }

    public abstract class ValueEnablerEditCommands : DoEditCommands
    {
        private readonly DescriptionVM _Description;

        protected ValueEnablerEditCommands(ValueEnablerVM valueEnabler)
            : base(valueEnabler)
        {
            Name = valueEnabler.Name;
            Owner = valueEnabler;
            _Description = new DescriptionVM(valueEnabler.Description.Description.Clone() as Description);
        }

        public string Name { get; set; }
        public DescriptionVM Description => _Description;

        protected void UpdateDescription()
            => (Owner as ValueEnablerVM).Description.ReplaceDescriptions(Description);
    }
}
