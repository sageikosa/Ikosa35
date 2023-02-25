using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Informational : Adjunct, IInformable
    {
        public Informational(object source) :
            base(source)
        {
        }

        #region IInformable Members
        public IEnumerable<Info> Inform(CoreActor observer)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override object Clone()
        {
            return new Informational(Source);
        }
    }
}
