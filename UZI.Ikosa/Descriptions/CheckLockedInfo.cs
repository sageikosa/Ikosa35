using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Descriptions
{
    /// <summary>Description that can be unlocked by making a successful skill check</summary>
    [Serializable]
    public class CheckLockedInfo : Info
    {
        #region Construction
        public CheckLockedInfo(){
        }

        public CheckLockedInfo(Info information, IList<SuccessCheckMarker> checkProperties)
        {
            this.Message = information.Message;
            _Info = information;
            _CheckProps = checkProperties;
        }
        #endregion

        #region Private Data
        private Info _Info;
        private IList<SuccessCheckMarker> _CheckProps;
        #endregion

        public Info Information { get { return _Info; } }
        public IList<SuccessCheckMarker> CheckProperties { get { return _CheckProps; } }

        public override object Clone()
        {
            return new CheckLockedInfo
            {
                Message = this.Message,
                _Info = Information,
                _CheckProps = this.CheckProperties
            };
        }
    }
}
