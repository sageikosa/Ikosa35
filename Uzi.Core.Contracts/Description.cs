using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    /// <summary>Concrete implementation of IDescription; a title and one or more descriptions</summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class Description : Info
    {
        #region Construction
        public Description()
            : base()
        {
        }

        /// <summary>Concrete implementation of IDescription; a title and one or more descriptions</summary>
        public Description(string title, params string[] information)
        {
            Message = title;
            _Info = information;
        }
        #endregion

        #region state
        private string[] _Info;
        #endregion

        [DataMember]
        public string[] Descriptions { get => _Info; set => _Info = value; }

        /// <summary>Copies string and array</summary>
        public override object Clone()
            => new Description(Message, Descriptions.Select(_d => _d).ToArray());
    }
}
