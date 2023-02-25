using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    public class PrerequisiteActorsEventArgs : EventArgs
    {
        public PrerequisiteActorsEventArgs(List<Guid> actors)
            : base()
        {
            this.Actors = actors;
        }

        public List<Guid> Actors { get; set; }
    }
}
