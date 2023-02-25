using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class OptionAimInfo : AimingModeInfo
    {
        public OptionAimInfo()
        {
        }

        //public OptionAimInfo(OptionAim optionAim, OptionAimOption option, CoreAction action, CoreActor actor)
        //    : base(optionAim, action, actor)
        //{
        //    Options = optionAim.ListOptions
        //        .Where(_o => _o.Key==option.Key)
        //        .Select(_o => new OptionAimOption
        //        {
        //            Key = _o.Key,
        //            Description = _o.Description,
        //            Name = _o.Name
        //        }).ToArray();
        //    NoDuplicates = optionAim.NoDuplicates;
        //}

        [DataMember]
        public OptionAimOption[] Options { get; set; }
        [DataMember]
        public bool NoDuplicates { get; set; }

        public OptionAimOption FirstOption { get { return Options.FirstOrDefault(); } }
    }
}
