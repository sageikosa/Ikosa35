using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public class InfoFeedback : InteractionFeedback
    {
        public InfoFeedback(object source, Info informations)
            : base(source)
        {
            _Info = informations;
        }

        private Info _Info;
        public Info Information => _Info;
    }
}
