using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DeltaCalcInfo
    {
        public DeltaCalcInfo(Guid principalID, string title)
        {
            PrincipalID = principalID;
            Title = title;
        }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public Guid PrincipalID { get; set; }
        [DataMember]
        public int BaseValue { get; set; }
        [DataMember]
        public int Result { get; set; }
        [DataMember]
        public List<DeltaInfo> Deltas { get; set; }

        /// <summary>Sets BaseValue and Result</summary>
        public void SetBaseResult(int value)
            => BaseValue = Result = value;

        /// <summary>Adds constructed DeltaInfo, but doesn't alter Result</summary>
        public void AddDelta(string name, int value)
        {
            if (Deltas == null)
                Deltas = new List<DeltaInfo>();
            Deltas.Add(new DeltaInfo { Name = name, Value = value });
        }

        /// <summary>Adds DeltaInfo, but doesn't alter Result</summary>
        public void AddDelta(DeltaInfo delta)
        {
            if (Deltas == null)
                Deltas = new List<DeltaInfo>();
            Deltas.Add(delta);
        }

        public DeltaCalcInfo Copy(Guid id, string title)
        {
            var _info = new DeltaCalcInfo(id, title)
            {
                BaseValue = BaseValue,
                Result = Result
            };
            if (Deltas != null)
                foreach (var _d in Deltas)
                {
                    _info.AddDelta(_d);
                }
            return _info;
        }
    }
}
