using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class StoryInfoElement
    {
        private readonly Guid _ID;
        private Info _Info;

        public StoryInfoElement(Info info)
            : this(Guid.NewGuid(), info)
        {
        }

        public StoryInfoElement(Guid id, Info info)
        {
            _ID = id;
            _Info = info;
        }

        public Guid ID => _ID;
        public Info Info { get => _Info; set => _Info = value; }
    }
}
