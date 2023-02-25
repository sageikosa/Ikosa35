using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    public class RequirementKey
    {
        public RequirementKey(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
        public virtual RequirementKeyInfo ToRequirementKeyInfo()
            => new RequirementKeyInfo
            {
                IndexKey = 0,
                Name = Name
            };

        public virtual bool IsKey(RequirementKeyInfo keyInfo)
            => keyInfo?.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public class RequirementValueKey<KeyType> : RequirementKey
    {
        public RequirementValueKey(string name, KeyType key)
            : base(name)
        {
            Key = key;
        }

        public KeyType Key { get; private set; }

        public override RequirementKeyInfo ToRequirementKeyInfo()
            => new RequirementKeyInfo
            {
                IndexKey = (Key is int _key) ? _key : 0,
                Name = Name
            };

        public override bool IsKey(RequirementKeyInfo keyInfo)
            => base.IsKey(keyInfo) 
            && (keyInfo.IndexKey == ((Key is int _key) ? _key : 0));
    }
}
