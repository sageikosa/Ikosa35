using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>Provides one or more IInfos regardless of the observer.</summary>
    [Serializable]
    public class Informable : IInformable
    {
        /// <summary>Provides one IInfo regardless of the observer.</summary>
        public Informable(params Info[] informable)
        {
            _Infos = informable.ToList();
        }

        private List<Info> _Infos;

        public List<Info> Infos { get { return _Infos; } }

        /// <summary>Enumerates all IInfos</summary>
        public IEnumerable<Info> Inform(CoreActor observer) { return _Infos; }
    }
}
