using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public class NamedKeyVM : ModuleManagementVMBase
    {
        private readonly NamedKeysPartVM _Part;
        private readonly Guid _KeyID;
        private string _Name;

        public NamedKeyVM(NamedKeysPartVM part, Guid keyID, string name)
        {
            _Part = part;
            _KeyID = keyID;
            _Name = name;
        }

        public NamedKeysPartVM Part => _Part;
        public Guid KeyID => _KeyID;

        public string Name
        {
            get => _Name;
            set
            {
                // track
                _Name = value;

                // rename in module
                Part.Part.Module.NamedKeyGuids[KeyID] = Name;
                DoPropertyChanged(nameof(Name));
            }
        }

        public override Commandable GetDefaultCommands()
            => new EditNamedKeyCommands(this);
    }

    public class EditNamedKeyCommands : EditCommands
    {
        public EditNamedKeyCommands(NamedKeyVM namedKey)
        {
            NamedKey = namedKey;
            EditCommand = new RelayCommand<object>(target =>
            {
                namedKey.Commands = new DoEditNamedKeyCommands(namedKey);
            });
        }

        public NamedKeyVM NamedKey { get; set; }
    }

    public class DoEditNamedKeyCommands : DoEditCommands
    {
        public DoEditNamedKeyCommands(NamedKeyVM namedKey)
            : base(namedKey)
        {
            Name = namedKey.Name;
            DoEditCommand = new RelayCommand(() =>
            {
                namedKey.Name = Name;
                namedKey.Commands = namedKey.GetDefaultCommands();
            });
        }
        public string Name { get; set; }
    }
}
